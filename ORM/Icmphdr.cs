using System;

namespace snortdb
{
    public class Icmphdr
    {
        public Icmphdr()
        {
        }
        public Icmphdr(UInt16 icmp_type, int icmp_code, int icmp_csum, int icmp_id, int icmp_seq)
        {
            this.icmp_type = icmp_type;
            this.icmp_code = icmp_code;
            this.icmp_csum = icmp_csum;
            this.icmp_id = icmp_id;
            this.icmp_seq = icmp_seq;
        }


        public UInt16 icmp_type { get; set; }
        public int icmp_code { get; set; }
        public int icmp_csum { get; set; }
        public int icmp_id { get; set; }
        public int icmp_seq { get; set; }
        public string icmp_type_text { get; set; }

    }
}