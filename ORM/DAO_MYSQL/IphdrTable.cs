using MySql.Data.MySqlClient;
using System;
using snortdb;

namespace razor.ORM.DAO_MYSQL
{
    public class IphdrTable
    {
        public static Iphdr GetIphdr(int cid, int sid, MySqlConnection conn)
        {
            Iphdr iphdr = new Iphdr();
            UInt32 schema = AlertMapper.GetSchemaID(conn);
            using (conn)
            {
                conn.Open();
                MySqlCommand cmd;

                cmd = new MySqlCommand("SELECT * FROM iphdr WHERE cid = " + cid.ToString() + " AND sid = " + sid.ToString(), conn);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        iphdr.sid = sid;
                        iphdr.cid = cid;

                        iphdr.ip_ver = reader.GetInt32("ip_ver");

                        if (schema < 200) //UINT32 IP column
                        {
                            iphdr.source = AlertMapper.ResolveIP4(reader.GetUInt32("ip_src"));
                            iphdr.destination = AlertMapper.ResolveIP4(reader.GetUInt32("ip_dst"));
                        }
                        else
                        {

                            //IP v6
                            if ((iphdr.ip_ver) != 4)
                            {
                                iphdr.ip_src = new Byte[16];
                                iphdr.ip_dst = new Byte[16];
                                reader.GetBytes(reader.GetOrdinal("ip_src"), 0, iphdr.ip_src, 0, 16);
                                reader.GetBytes(reader.GetOrdinal("ip_dst"), 0, iphdr.ip_dst, 0, 16);
                            }
                            //IP v4
                            else
                            {
                                iphdr.ip_src = new Byte[4];
                                iphdr.ip_dst = new Byte[4];
                                reader.GetBytes(reader.GetOrdinal("ip_src"), 0, iphdr.ip_src, 0, 4);
                                reader.GetBytes(reader.GetOrdinal("ip_dst"), 0, iphdr.ip_dst, 0, 4);
                            }
                        }
                        iphdr.ip_hlen = reader.GetInt32("ip_hlen");
                        iphdr.ip_tos = reader.GetInt32("ip_tos");
                        iphdr.ip_ecn = iphdr.ip_tos & 3;
                        iphdr.ip_len = reader.GetInt32("ip_len");
                        iphdr.ip_id = reader.GetInt32("ip_id");
                        iphdr.ip_flags = reader.GetInt32("ip_flags");
                        iphdr.ip_off = reader.GetInt32("ip_off");
                        iphdr.ip_csum = reader.GetInt32("ip_csum");
                        iphdr.ip_ttl = reader.GetInt32("ip_ttl");
                        iphdr.ip_proto = reader.GetInt32("ip_proto");
                    }
                }
            }
            return iphdr;
        }
    }
}
