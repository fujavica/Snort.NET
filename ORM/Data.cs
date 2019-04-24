using System;

namespace snortdb
{
    [Serializable]
    public class Data
    {
        public Data()
        {
        }
        public Data(string hex, string ascii)
        {
            this.hex = hex;
            this.ascii = ascii;

        }
        public Data(string hex)
        {
            this.hex = hex;
        }


        public string hex { get; set; }
        public string ascii { get; set; }

    }
}