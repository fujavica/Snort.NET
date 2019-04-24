using System;

namespace snortdb
{
    public class Signature
    {
        public Signature()
        {
        }
        public Signature(UInt32 sig_id, int sig_class_id, int sig_priority, int sig_rev, int sig_sid, int sig_gid, string sig_name)
        {
            this.sig_id = sig_id;
            this.sig_class_id = sig_class_id;
            this.sig_priority = sig_priority;
            this.sig_rev = sig_rev;
            this.sig_sid = sig_sid;
            this.sig_gid = sig_gid;
            this.sig_name = sig_name;
        }

        public UInt32 sig_id { get; set; }
        public int sig_class_id { get; set; }
        public int sig_priority { get; set; }
        public int sig_rev { get; set; }
        public int sig_sid { get; set; }
        public int sig_gid { get; set; }
        public string sig_name { get; set; }
        public string class_name { get; set; }
        public string ref_url { get; set; }
    }
}