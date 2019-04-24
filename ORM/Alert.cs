using System;

namespace snortdb
{


    public class Alert
    {
        public Alert() { }

        public Alert(int prior, string src, string dst, UInt32 signature, DateTime time, int cid, int sid, int protocol, UInt16 subprotocol_dest, UInt16 subprotocol_src)
        {
            this.dest_ip = dst;
            this.src_ip = src;
            this.priority = prior;
            this.sig_id = signature;
            this.time = time;
            this.cid = cid;
            this.sid = sid;
            this.protocol = protocol;
            this.subprotocol_dest = subprotocol_dest;
            this.subprotocol_src = subprotocol_src;
        }


        public int priority { get; set; }
        public string src_ip { get; set; }
        public string dest_ip { get; set; }
        public UInt32 sig_id { get; set; }
        public DateTime time { get; set; }
        public int cid { get; set; }
        public int sid { get; set; }
        public int protocol { get; set; }
        public UInt16 subprotocol_dest { get; set; }
        public UInt16 subprotocol_src { get; set; }
        public string desc { get; set; }
    }

}


