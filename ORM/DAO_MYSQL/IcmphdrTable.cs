using MySql.Data.MySqlClient;
using System;
using snortdb;

namespace razor.ORM.DAO_MYSQL
{
    public class IcmphdrTable
    {
        public static Icmphdr GetIcmphdr(int cid, int sid, MySqlConnection conn)
        {
            Icmphdr icmphdr = new Icmphdr();

            using (conn)
            {
                conn.Open();
                MySqlCommand cmd;

                cmd = new MySqlCommand("SELECT * FROM icmphdr WHERE cid = " + cid.ToString() + " AND sid = " + sid.ToString(), conn);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        icmphdr.icmp_type = reader.GetUInt16("icmp_type");
                        icmphdr.icmp_code = reader.GetInt32("icmp_code");
                        icmphdr.icmp_csum = reader.GetInt32("icmp_csum");
                        icmphdr.icmp_id = reader.GetInt32("icmp_id");
                        icmphdr.icmp_seq = reader.GetInt32("icmp_seq");
                    }
                }
            }
            return icmphdr;
        }
    }
}
