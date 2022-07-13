# POC-KAFKA-CDC
Processamento de dados em realtime entre parceiros da Educação
![REALTIME SED](https://user-images.githubusercontent.com/35972748/178076003-99b5c1cc-1a98-41d4-8f50-d5608b7a6227.jpg)

### Configure e inicie o conector de origem do Debezium PostgreSQL

Crie um arquivo de configuração (`pg-source-connector.json`) para o conector de origem PostgreSQL - substitua os valores de acordo com sua instância do Azure PostgreSQL.

``` JSON
{
    "name": "todo-conector",
    "configurar": {
        "connector.class": "io.debezium.connector.postgresql.PostgresConnector",
        "database.hostname": "<substituir pelo nome da instância do Azure PostgreSQL>.postgres.database.azure.com",
        "database.port": "5432",
        "database.user": "<substituir pelo nome de usuário do banco de dados>",
        "database.password": "<substituir pela senha do banco de dados>",
        "database.dbname": "postgres",
        "database.server.name": "meu-servidor",
        "plugin.name": "wal2json",
        "table.whitelist": "public.todos"
    }
}
```

> [!DICA]
> O atributo `database.server.name` é um nome lógico que identifica e fornece um namespace para o servidor/cluster de banco de dados PostgreSQL específico que está sendo monitorado. Para informações detalhadas, consulte [documentação do Debezium](https://debezium.io/documentation /reference/1.2/connectors/postgresql.html#postgresql-property-database-server-name)

Para criar uma instância do conector, use o endpoint da API REST do Kafka Connect:

```bash
curl -X POST -H "Content-Type: application/json" --data @pg-source-connector.json http://localhost:8083/connectors
```

To check the status of the connector:

```bash
curl -s http://localhost:8083/connectors/todo-connector/status
```
## Teste a captura de dados de alteração
Para ver a captura de dados alterados em ação, você precisará criar/atualizar/excluir registros no banco de dados Azure PostgreSQL.

Comece conectando-se ao seu banco de dados Azure PostgreSQL (o exemplo abaixo usa [psql](https://www.postgresql.org/docs/12/app-psql.html))

```bash
psql -h <POSTGRES_INSTANCE_NAME>.postgres.database.azure.com -p 5432 -U <POSTGRES_USER_NAME> -W -d <POSTGRES_DB_NAME> --set=sslmode=require

por exemplo.

psql -h my-postgres.postgres.database.azure.com -p 5432 -U testuser@my-postgres -W -d postgres --set=sslmode=require
```

**Crie uma tabela e insira registros**

```sql
CREATE TABLE todos (id SERIAL, descrição VARCHAR(50), todo_status VARCHAR(12), PRIMARY KEY(id));

INSERT INTO todos (descrição, todo_status) VALUES ('configuração do postgresql no azure', 'completo');
INSERT INTO todos (descrição, todo_status) VALUES ('configuração do kafka connect', 'completo');
INSERT INTO todos (descrição, todo_status) VALUES ('configurar e instalar conector', 'em andamento');
INSERT INTO todos (descrição, todo_status) VALUES ('iniciar conector', 'pendente');
```

O conector deve agora entrar em ação e enviar eventos de dados alterados para um tópico do Event Hubs com o seguinte nome `my-server.public.todos`, supondo que você tenha `my-server` como o valor para `database.server.name` e `public.todos` é a tabela cujas alterações você está acompanhando (de acordo com a configuração `table.whitelist`)

**Verifique o tópico dos Hubs de Eventos**

Vamos fazer uma introspecção do conteúdo do tópico para garantir que tudo esteja funcionando conforme o esperado. O exemplo abaixo usa [`kafkacat`](https://github.com/Azure/azure-event-hubs-for-kafka/tree/master/quickstart/kafkacat), mas você também pode [criar um consumidor usando qualquer um dos as opções listadas aqui](apache-kafka-developer-guide.md)

Crie um arquivo chamado `kafkacat.conf` com o seguinte conteúdo:

```
metadata.broker.list=<digite o namespace dos hubs de eventos>.servicebus.windows.net:9093
security.protocol=SASL_SSL
sasl.mechanisms=PLANO
sasl.username=$ConnectionString
sasl.password=<digite a string de conexão dos hubs de eventos>
```

> [!NOTA]
> Atualize os atributos `metadata.broker.list` e `sasl.password` em `kafkacat.conf` de acordo com as informações dos Hubs de Eventos.

Em um terminal diferente, inicie um consumidor:

```bash
export KAFKACAT_CONFIG=kafkacat.conf
export BROKER=<digite o namespace dos hubs de eventos>.servicebus.windows.net:9093
export TOPIC=my-server.public.todos

kafkacat -b $BROKER -t $TOPIC -o início
```
Agora que as mudanças da tabela `todos` estão sendo capturadas no tópico Event Hubs, você usará o conector FileStreamSink (disponível por padrão no Kafka Connect) para consumir esses eventos.




Crie um arquivo de configuração (`file-sink-connector.json`) para o conector - substitua o atributo `file` de acordo com seu sistema de arquivos

```json
{
    "name": "cdc-file-sink",
    "config": {
        "connector.class": "org.apache.kafka.connect.file.FileStreamSinkConnector",
        "tasks.max": "1",
        "topics": "my-server.public.todos",
        "file": "<enter full path to file e.g. /Users/foo/todos-cdc.txt>"
    }
}
```

To create the connector and check its status:

```bash
curl -X POST -H "Content-Type: application/json" --data @file-sink-connector.json http://localhost:8083/connectors

curl http://localhost:8083/connectors/cdc-file-sink/status
```

Insert/update/delete database records and monitor the records in the configured output sink file:

```bash
tail -f /Users/foo/todos-cdc.txt
```
