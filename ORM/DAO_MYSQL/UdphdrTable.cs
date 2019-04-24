using MySql.Data.MySqlClient;
using System;
using snortdb;

namespace razor.ORM.DAO_MYSQL
{
    public class UdphdrTable
    {
        public static Udphdr GetUdphdr(int cid, int sid, MySqlConnection conn)
        {

            Udphdr udphdr = new Udphdr();

            using (conn)
            {
                conn.Open();
                MySqlCommand cmd;

                cmd = new MySqlCommand("SELECT * FROM udphdr WHERE cid = " + cid.ToString() + " AND sid = " + sid.ToString(), conn);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        udphdr.udp_sport = reader.GetUInt16("udp_sport");
                        udphdr.udp_dport = reader.GetUInt16("udp_dport");
                        udphdr.udp_len = reader.GetInt32("udp_len");
                        udphdr.udp_csum = reader.GetInt32("udp_csum");

                    }
                }
            }
            return udphdr;
        }
    }
}
