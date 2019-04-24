using System;

namespace snortdb
{
    public class Tcphdr
    {
        public Tcphdr()
        {
        }
        public Tcphdr(UInt16 tcp_sport, UInt16 tcp_dport, Int64 tcp_seq, Int64 tcp_ack, int tcp_off, int tcp_res, int tcp_flags, int tcp_win, int tcp_csum, int tcp_urp)
        {
            this.tcp_sport = tcp_sport;
            this.tcp_dport = tcp_dport;
            this.tcp_seq = tcp_seq;
            this.tcp_ack = tcp_ack;
            this.tcp_off = tcp_off;
            this.tcp_res = tcp_res;
            this.tcp_flags = tcp_flags;
            this.tcp_win = tcp_win;
            this.tcp_csum = tcp_csum;
            this.tcp_urp = tcp_urp;
        }


        public UInt16 tcp_sport { get; set; }
        public UInt16 tcp_dport { get; set; }
        public Int64 tcp_seq { get; set; }
        public Int64 tcp_ack { get; set; }
        public int tcp_off { get; set; }
        public int tcp_res { get; set; }
        public int tcp_flags { get; set; }
        public int tcp_win { get; set; }
        public int tcp_csum { get; set; }
        public int tcp_urp { get; set; }
        public string tcp_protocol { get; set; }
        public string tcp_protocol2 { get; set; }
        public string tcp_flags_str { get; set; }

    }
}