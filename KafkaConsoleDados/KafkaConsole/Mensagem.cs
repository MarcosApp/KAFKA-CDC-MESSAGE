using System;
using System.Collections.Generic;
using System.Text;

namespace KafkaConsole
{
    public class Mensagem
    {
        public SchemaPrimary schema { get; set; }
        public Payload payload { get; set; }
    }

    public class SchemaPrimary
    {
        public string type { get; set; }
        public ICollection<FieldsPrimary> fields { get; set; }
        public bool optional { get; set; }
        public string name { get; set; }
    }

    public class FieldsPrimary
    {
        public string type { get; set; }
        public ICollection<FieldChield> fields { get; set; }
        public bool optional { get; set; }
        public string field { get; set; }
        public string name { get; set; }
    }

    public class FieldChield
    {
        public string type { get; set; }
        public bool optional { get; set; }
        public string field { get; set; }
    }

    public class Payload
    {
        public Entities before { get; set; }
        public Entities after { get; set; }
    }

    public class Entities
    {
        public int Id { get; set; }
        public string NOME { get; set; }
        public string TURMA { get; set; }
    }

    public class KeyIndentification
    {
        public Identification payload { get; set; }
    }

    public class Identification
    {
        public int Id { get; set; }
    }
}
