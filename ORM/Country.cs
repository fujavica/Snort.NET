using System;

namespace snortdb
{
    public class Country
    {
        public Country()
        {
        }
        public Country(string code, string name, int count)
        {
            this.code = code;
            this.name = name;
            this.count = count;
        }


        public string code { get; set; }
        public string name { get; set; }
        public int count { get; set; }
    }
}