using MySql.Data.MySqlClient;
using System;
using snortdb;

namespace razor.ORM.DAO_MYSQL
{
    public class TcphdrTable
    {
        public static Tcphdr GetTcphdr(int cid, int sid, MySqlConnection conn)
        {
            Tcphdr tcphdr = new Tcphdr();

            using (conn)
            {
                conn.Open();
                MySqlCommand cmd;

                cmd = new MySqlCommand("SELECT * FROM tcphdr WHERE cid = " + cid.ToString() + " AND sid = " + sid.ToString(), conn);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tcphdr.tcp_sport = reader.GetUInt16("tcp_sport");
                        tcphdr.tcp_dport = reader.GetUInt16("tcp_dport");
                        tcphdr.tcp_seq = reader.GetInt64("tcp_seq");
                        tcphdr.tcp_ack = reader.GetInt64("tcp_ack");
                        tcphdr.tcp_off = reader.GetInt32("tcp_off");
                        tcphdr.tcp_res = reader.GetInt32("tcp_res");
                        tcphdr.tcp_flags = reader.GetInt32("tcp_flags");
                        tcphdr.tcp_flags_str = Convert.ToString(tcphdr.tcp_flags, 2).PadLeft(9, '0');
                        tcphdr.tcp_win = reader.GetInt32("tcp_win");
                        tcphdr.tcp_csum = reader.GetInt32("tcp_csum");
                        tcphdr.tcp_urp = reader.GetInt32("tcp_urp");
                    }
                }
            }
            return tcphdr;
        }
    }
}
