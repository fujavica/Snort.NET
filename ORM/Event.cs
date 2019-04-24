using System;

namespace snortdb
{
    public class Event
    {
        public Event()
        {
        }
        public Event(int sid, int cid, int signature, DateTime timestamp)
        {
            this.sid = sid;
            this.cid = cid;
            this.signature = signature;
            this.timestamp = timestamp;
        }


        public int sid { get; set; }
        public int cid { get; set; }
        public int signature { get; set; }
        public DateTime timestamp { get; set; }
    }
}