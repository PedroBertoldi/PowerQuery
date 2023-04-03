using System;

namespace PowerQuery.Models
{
    public class PowerQueryConfig
    {
        private int _maxExpanssionLevel = 15;
        public int MaxExpanssionLevel
        {
            get
            {
                return _maxExpanssionLevel;
            }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException($"{nameof(MaxExpanssionLevel)} must be bigger or equal to 1");
                }
                _maxExpanssionLevel = value;
            }
        }
        public bool FilterInList { get; set; } = true;
        public Type[] ExcludeByType { get; set; } = new Type[0];
        public string[] ExcludeByName { get; set; } = new string[0];
        public bool CaseSensitive { get; set; } = false;
    }
}
