using System;

namespace snortdb
{
    public class Sensor
    {
        public Sensor()
        {
        }

        public Sensor(int sid, string hostname, string iface, string filter, int last_cid)
        {
            this.sid = sid;
            this.hostname = hostname;
            this.iface = iface;
            this.filter = filter;
            this.last_cid = last_cid;
        }

        public int sid { get; set; }
        public string hostname { get; set; }
        public string iface { get; set; }
        public string filter { get; set; }
        public int last_cid { get; set; }
    }
}


