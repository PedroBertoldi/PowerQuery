namespace PowerQueryV2
{
    public class PQConfig
    {
        public int MaxExpanssionLevel { get; set; } = 1;
        public string[] Exclude { get; set; } = new string[0];
        public bool SeparateString { get; set; } = true;
    }
}
