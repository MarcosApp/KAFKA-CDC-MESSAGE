using Confluent.Kafka;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace KafkaConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var consumerConfig = new ConsumerConfig
            {
                GroupId = $"dbserver1.dbo.TESTE_KAFKA_CDC.{Guid.NewGuid():N}.group.id",
                BootstrapServers = "192.168.1.80:9092",
                AutoOffsetReset = AutoOffsetReset.Earliest,
            };


            using (var c = new ConsumerBuilder<string, string>(consumerConfig).Build())
            {
                c.Subscribe("dbserver1.dbo.TESTE_KAFKA_CDC");
                try
                {
                    while (true)
                    {
                        try
                        {
                            var cr = c.Consume();
                            var a = JsonSerializer.Serialize(cr.Message);
                            Mensagem b = null;

                            if (cr.Message.Value != null)
                                b = JsonSerializer.Deserialize<Mensagem>(cr.Message?.Value);

                            var chave = JsonSerializer.Deserialize<KeyIndentification>(cr.Key);
                            var offset = cr.TopicPartitionOffset;

                            if (b == null)
                            {
                                Console.WriteLine($"DELETADO - {chave.payload.Id}");
                            }
                            else
                            {
                                if (b.payload?.after != null && b.payload?.before == null)
                                    Console.WriteLine($"INCLUSÃO - ID:{b.payload?.after.Id}\nNOME:{b.payload?.after.NOME}\nTURMA:{b.payload?.after.TURMA}\n");
                                else
                                {
                                    if (b.payload?.after == null)
                                        Console.WriteLine($"ALTERAÇÃO - ID:{b.payload?.before.Id}\nNOME:{b.payload?.before.NOME}\nTURMA:{b.payload?.before.TURMA}\n");
                                    else
                                        Console.WriteLine($"ALTERAÇÃO - ID:{b.payload?.after.Id}\nNOME:{b.payload?.after.NOME}\nTURMA:{b.payload?.after.TURMA}\n");
                                }
                            }

                            Console.WriteLine("======================================================== \n");

                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw e;
                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }           
    }
}
