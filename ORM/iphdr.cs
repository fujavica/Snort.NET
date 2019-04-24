using System;

namespace snortdb
{
    public class Iphdr
    {
        public Iphdr()
        {
            source = null;
            destination = null;
        }
        public Iphdr(int sid, int cid, Byte[] ip_src, Byte[] ip_dst, int ip_ver, int ip_hlen, int ip_tos, int ip_len, int ip_id, int ip_flags, int ip_ttl, int ip_proto, int ip_csum)
        {
            this.sid = sid;
            this.cid = cid;
            this.ip_src = ip_src;
            this.ip_dst = ip_dst;
            this.ip_hlen = ip_hlen;
            this.ip_tos = ip_tos;
            this.ip_len = ip_len;
            this.ip_id = ip_id;
            this.ip_flags = ip_flags;
            this.ip_off = ip_off;
            this.ip_ver = ip_ver;
            this.ip_ttl = ip_ttl;
            this.ip_proto = ip_proto;
            this.ip_csum = ip_csum;
        }


        public int sid { get; set; }
        public int cid { get; set; }
        public Byte[] ip_src { get; set; }
        public Byte[] ip_dst { get; set; }
        public int ip_ver { get; set; }
        public int ip_hlen { get; set; }

        public int ip_tos { get; set; }
        public int ip_ecn { get; set; }
        public int ip_len { get; set; }
        public int ip_id { get; set; }
        public int ip_flags { get; set; }
        public int ip_off { get; set; }
        public int ip_ttl { get; set; }
        public int ip_proto { get; set; }
        public int ip_csum { get; set; }
        public string source { get; set; }
        public string destination { get; set; }
        public string protocol { get; set; }


    }
}