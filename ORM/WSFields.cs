using System;

namespace snortdb
{
    public class WSFields
    {
        public WSFields()
        {
        }
        public WSFields(int number, string time, string source, string destination, string protocol, string length)
        {
            this.number = number;
            this.time = time;
            this.source = source;
            this.destination = destination;
            this.protocol = protocol;
            this.length = length;
        }


        public long timestamp { get; set; }
        public int number { get; set; }
        public string time { get; set; }
        public string source { get; set; }
        public string destination { get; set; }
        public string protocol { get; set; }
        public string length { get; set; }
        public string info { get; set; }

    }
}