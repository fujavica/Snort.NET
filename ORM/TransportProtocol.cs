using System;

namespace snortdb
{
    [Serializable()]
    public class TransportProtocol
    {
        public TransportProtocol()
        {
        }
        public TransportProtocol(int number, string protocol, string name, string xref = null)
        {
            this.protocol = protocol;
            this.number = number;
            this.name = name;
            this.xref = xref;
        }

        public int number { get; set; }
        public string protocol { get; set; }
        public string name { get; set; }
        public string xref { get; set; }

    }
}