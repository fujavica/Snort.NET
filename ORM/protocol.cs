using System;

namespace snortdb
{
    public class Protocol
    {
        public Protocol()
        {
        }
        public Protocol(int pid, string name, string shortname, string reference = null)
        {
            this.pid = pid;
            this.name = name;
            this.shortname = shortname;
        }

        public int pid { get; set; }
        public string name { get; set; }
        public string shortname { get; set; }
        public string reference { get; set; }

    }
}