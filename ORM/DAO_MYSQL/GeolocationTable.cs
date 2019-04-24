using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using snortdb;


namespace razor.ORM.DAO_MYSQL
{
    public class geolocationTable
    {
        public static void InsertIP(string ip, string country, MySqlConnection conn)
        {
            using (conn)
            {
                conn.Open();
                MySqlCommand cmd;

                cmd = new MySqlCommand("INSERT INTO geolocation (ip, country) VALUES(\'" + ip + "\',\'" + country + "\')", conn);
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception) { }
            }

        }

        public static List<string> GetNewIPs(DateTime lastTime, MySqlConnection conn)
        {
            List<string> ips = new List<string>();

            using (conn)
            {
                conn.Open();
                MySqlCommand cmd;

                cmd = new MySqlCommand("SELECT DISTINCT i.ip_ver, i.ip_src as ip FROM event e JOIN iphdr i ON e.cid = i.cid AND e.sid = i.sid WHERE e.timestamp > '"
                + lastTime.ToString("yyyy-MM-dd HH:mm:ss") + "'", conn);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    Byte[] IP_bytes6 = new byte[16];
                    Byte[] IP_bytes4 = new byte[4];
                    while (reader.Read())
                    {
                        //IP v6
                        if (reader.GetInt32("ip_ver") != 4)
                        {
                            reader.GetBytes(reader.GetOrdinal("ip_src"), 0, IP_bytes6, 0, 16);
                            ips.Add(AlertMapper.ResolveIP(IP_bytes6));
                        }
                        else
                        {
                            reader.GetBytes(reader.GetOrdinal("ip_src"), 0, IP_bytes4, 0, 4);
                            ips.Add(AlertMapper.ResolveIP(IP_bytes6));
                        }
                    }
                }
            }
            return ips;
        }

        public static void LoadIPLocations(ref Dictionary<string, string> iplocs, MySqlConnection conn)
        {
            using (conn)
            {
                conn.Open();
                MySqlCommand cmd;

                cmd = new MySqlCommand("SELECT ip,country FROM geolocation", conn);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string ip = reader.GetString("ip");
                        if (!iplocs.ContainsKey(ip))
                        {
                            string country = reader.GetString("country");
                            iplocs.Add(ip, country);
                        }
                        else
                        {
                            string country = reader.GetString("country");
                            iplocs[ip] = country;
                        }
                    }
                }
            }

        }
    }
}
