

using System.Text.Json.Serialization;

namespace TesteRuntimeQuery.Models
{
    public class TestChildModel
    {
        public int Id { get; set; }
        public int Prop1 { get; set; }
        public string Prop2 { get; set; }

        [JsonIgnore]
        public TestModel TestModel { get; set; }
    }
}
