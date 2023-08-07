using System.Linq.Expressions;

namespace PowerQueryV2
{
    internal class PQTemp
    {
        public int CurrentLevel { get; set; } = 0;
        public string Path { get; set; } = string.Empty;
        public Expression[] TermArray { get; set; }
    }
}
