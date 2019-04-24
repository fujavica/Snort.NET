using System;

namespace snortdb
{
    public class Udphdr
    {
        public Udphdr()
        {
        }
        public Udphdr(UInt16 udp_sport, UInt16 udp_dport, int udp_len, int udp_csum)
        {
            this.udp_sport = udp_sport;
            this.udp_dport = udp_dport;
            this.udp_len = udp_len;
            this.udp_csum = udp_csum;
        }


        public UInt16 udp_sport { get; set; }
        public UInt16 udp_dport { get; set; }
        public int udp_len { get; set; }
        public int udp_csum { get; set; }
        public string udp_protocol { get; set; }
        public string udp_protocol2 { get; set; }

    }
}