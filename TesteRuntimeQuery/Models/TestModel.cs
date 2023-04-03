using System.Collections.Generic;

namespace TesteRuntimeQuery.Models
{
    public class TestModel
    {
        public TestModel()
        {
            TestChildModels = new List<TestChildModel>();
        }
        public int Id { get; set; }
        public string Prop1 { get; set; }
        public string Prop2 { get; set; }
        public IList<TestChildModel> TestChildModels { get; set; }
        public TestModel2 Model2 { get; set; }
    }
}
