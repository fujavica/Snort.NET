using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using snortdb;
using System.Net;


namespace razor.ORM.DAO_MYSQL
{
    public class AlertMapper
    {
        static HashSet<string> IPs = new HashSet<string>();

        public static UInt32 GetSchemaID(MySqlConnection conn)
        {
            UInt32 schema = 0;
            using (conn)
            {
                conn.Open();
                MySqlCommand cmd;
                cmd = new MySqlCommand("SELECT vseq FROM `schema` LIMIT 1", conn);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        schema = reader.GetUInt32("vseq");
                    }
                }
            }
            return schema;
        }
        public static List<Alert> ResolveAlerts(int limit, ref Dictionary<UInt32, string> signatureStrings, MySqlConnection conn)
        {
            if (signatureStrings == null) signatureStrings = new Dictionary<UInt32, string>();
            List<Alert> list = new List<Alert>();
            UInt32 schema = GetSchemaID(conn);
            using (conn)
            {
                conn.Open();

                MySqlCommand cmd;

                if (limit == 0)
                {
                    cmd = new MySqlCommand("SELECT e.sid, e.cid, e.signature, e.timestamp, i.ip_ver, i.ip_src, i.ip_dst, i.ip_proto, s.sig_priority, s.sig_name, " +
                        "COALESCE(t.tcp_dport, u.udp_dport, ic.icmp_type, 0) as subprotocol, COALESCE(t.tcp_sport, u.udp_sport, ic.icmp_type, 0) as subprotocol2 FROM event e JOIN iphdr i ON e.cid = i.cid AND e.sid = i.sid " +
                        "JOIN signature s ON e.signature = s.sig_id LEFT JOIN tcphdr t ON e.cid = t.cid AND e.sid = t.sid LEFT JOIN udphdr u ON e.cid = u.cid AND e.sid = u.sid " +
                        "LEFT JOIN icmphdr ic ON e.cid = ic.cid AND e.sid = ic.sid ORDER BY e.timestamp DESC", conn);
                }
                else
                {
                    cmd = new MySqlCommand("SELECT e.sid, e.cid, e.signature, e.timestamp, i.ip_ver, i.ip_src, i.ip_dst, i.ip_proto, s.sig_priority, s.sig_name, " +
                        "COALESCE(t.tcp_dport, u.udp_dport, ic.icmp_type, 0) as subprotocol, COALESCE(t.tcp_sport, u.udp_sport, ic.icmp_type, 0) as subprotocol2 FROM event e JOIN iphdr i ON e.cid = i.cid AND e.sid = i.sid " +
                        "JOIN signature s ON e.signature = s.sig_id LEFT JOIN tcphdr t ON e.cid = t.cid AND e.sid = t.sid LEFT JOIN udphdr u ON e.cid = u.cid AND e.sid = u.sid " +
                        "LEFT JOIN icmphdr ic ON e.cid = ic.cid AND e.sid = ic.sid ORDER BY e.timestamp DESC LIMIT " + limit.ToString(), conn);
                }
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    int sid;
                    int cid;
                    UInt32 sig_id;
                    string signature;
                    string src_ip;
                    string dest_ip;
                    Byte[] IP_bytes6 = new byte[16];
                    Byte[] IP_bytes4 = new byte[4];
                    string new_ip;
                    //string new_ip;

                    while (reader.Read())
                    {
                        sid = reader.GetInt32("sid");
                        cid = reader.GetInt32("cid");
                        sig_id = reader.GetUInt32("signature");
                        signature = reader.GetString("sig_name");

                        if (schema < 200)           //UINT32
                        {
                            UInt32 ip = reader.GetUInt32("ip_src");
                            new_ip = ResolveIP4(ip);
                            if (!IPs.TryGetValue(new_ip, out src_ip))
                            {
                                IPs.Add(new_ip);
                                src_ip = new_ip;
                            }
                            ip = reader.GetUInt32("ip_dst");
                            new_ip = ResolveIP4(ip);
                            if (!IPs.TryGetValue(new_ip, out dest_ip))
                            {
                                IPs.Add(new_ip);
                                dest_ip = new_ip;
                            }
                        }
                        else        //VARBINARY(16)
                        {

                            //IP v6
                            if (reader.GetInt32("ip_ver") != 4)
                            {
                                reader.GetBytes(reader.GetOrdinal("ip_src"), 0, IP_bytes6, 0, 16);
                                new_ip = ResolveIP(IP_bytes6);
                                if (!IPs.TryGetValue(new_ip, out src_ip))
                                {
                                    IPs.Add(new_ip);
                                    src_ip = new_ip;
                                }
                                reader.GetBytes(reader.GetOrdinal("ip_dst"), 0, IP_bytes6, 0, 16);
                                new_ip = ResolveIP(IP_bytes6);
                                if (!IPs.TryGetValue(new_ip, out dest_ip))
                                {
                                    IPs.Add(new_ip);
                                    dest_ip = new_ip;
                                }
                            }
                            //IP v4
                            else
                            {
                                reader.GetBytes(reader.GetOrdinal("ip_src"), 0, IP_bytes4, 0, 4);
                                new_ip = ResolveIP(IP_bytes4);
                                if (!IPs.TryGetValue(new_ip, out src_ip))
                                {
                                    IPs.Add(new_ip);
                                    src_ip = new_ip;
                                }
                                reader.GetBytes(reader.GetOrdinal("ip_dst"), 0, IP_bytes4, 0, 4);
                                new_ip = ResolveIP(IP_bytes4);
                                if (!IPs.TryGetValue(new_ip, out dest_ip))
                                {
                                    IPs.Add(new_ip);
                                    dest_ip = new_ip;
                                }
                            }
                        }
                        if (signatureStrings.ContainsKey(sig_id))
                        {
                            signature = signatureStrings[sig_id];
                        }
                        else
                        {
                            signatureStrings.Add(sig_id, signature);
                        }
                        list.Add(new Alert()
                        {
                            sid = sid,
                            cid = cid,
                            time = reader.GetDateTime("timestamp"),
                            sig_id = sig_id,
                            src_ip = src_ip,
                            dest_ip = dest_ip,
                            protocol = reader.GetInt32("ip_proto"),
                            priority = reader.GetInt32("sig_priority"),
                            subprotocol_dest = reader.GetUInt16("subprotocol"),
                            subprotocol_src = reader.GetUInt16("subprotocol2"),
                            desc = signatureStrings[sig_id]
                        });
                    }
                }
            }
            return list;
        }

        public static List<Alert> UpdateAlerts(int sid, int start, Dictionary<UInt32, string> signatureStrings, MySqlConnection conn)
        {
            if (signatureStrings == null) signatureStrings = new Dictionary<UInt32, string>();
            List<Alert> list = new List<Alert>();
            UInt32 schema = GetSchemaID(conn);
            using (conn)
            {
                conn.Open();
                MySqlCommand cmd;
                cmd = new MySqlCommand("SELECT e.sid, e.cid, e.signature, e.timestamp, i.ip_ver, i.ip_src, i.ip_dst, i.ip_proto, s.sig_priority, s.sig_name, " +
                    "COALESCE(t.tcp_dport, u.udp_dport, ic.icmp_type, 0) as subprotocol, COALESCE(t.tcp_sport, u.udp_sport, ic.icmp_type, 0) as subprotocol2 FROM event e JOIN iphdr i ON e.cid = i.cid AND e.sid = i.sid " +
                    "JOIN signature s ON e.signature = s.sig_id LEFT JOIN tcphdr t ON e.cid = t.cid AND e.sid = t.sid LEFT JOIN udphdr u ON e.cid = u.cid AND e.sid = u.sid " +
                    "LEFT JOIN icmphdr ic ON e.cid = ic.cid AND e.sid = ic.sid WHERE e.sid =" + sid + " AND e.cid > " + start + " ORDER BY e.timestamp DESC LIMIT 100000", conn);

                // cmd = new MySqlCommand("SELECT * FROM event WHERE sid =" + sid + " AND cid > " + start, conn);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    int newcid;
                    int newsid;
                    UInt32 sig_id;
                    string signature;
                    string src_ip;
                    string dest_ip;
                    string new_ip;
                    Byte[] IP_bytes6 = new byte[16];
                    Byte[] IP_bytes4 = new byte[4];

                    while (reader.Read())
                    {
                        if (schema < 200) //UINT32
                        {
                            UInt32 ip = reader.GetUInt32("ip_src");
                            new_ip = ResolveIP4(ip);
                            if (!IPs.TryGetValue(new_ip, out src_ip))
                            {
                                IPs.Add(new_ip);
                                src_ip = new_ip;
                            }
                            ip = reader.GetUInt32("ip_dst");
                            new_ip = ResolveIP4(ip);
                            if (!IPs.TryGetValue(new_ip, out dest_ip))
                            {
                                IPs.Add(new_ip);
                                dest_ip = new_ip;
                            }
                        }
                        else
                        {
                            //IP v6
                            if (reader.GetInt32("ip_ver") != 4)
                            {
                                reader.GetBytes(reader.GetOrdinal("ip_src"), 0, IP_bytes6, 0, 16);
                                new_ip = ResolveIP(IP_bytes6);
                                if (!IPs.TryGetValue(new_ip, out src_ip))
                                {
                                    IPs.Add(new_ip);
                                    src_ip = new_ip;
                                }
                                reader.GetBytes(reader.GetOrdinal("ip_dst"), 0, IP_bytes6, 0, 16);
                                new_ip = ResolveIP(IP_bytes6);
                                if (!IPs.TryGetValue(new_ip, out dest_ip))
                                {
                                    IPs.Add(new_ip);
                                    dest_ip = new_ip;
                                }
                            }
                            //IP v4
                            else
                            {
                                reader.GetBytes(reader.GetOrdinal("ip_src"), 0, IP_bytes4, 0, 4);
                                new_ip = ResolveIP(IP_bytes4);
                                if (!IPs.TryGetValue(new_ip, out src_ip))
                                {
                                    IPs.Add(new_ip);
                                    src_ip = new_ip;
                                }
                                reader.GetBytes(reader.GetOrdinal("ip_dst"), 0, IP_bytes4, 0, 4);
                                new_ip = ResolveIP(IP_bytes4);
                                if (!IPs.TryGetValue(new_ip, out dest_ip))
                                {
                                    IPs.Add(new_ip);
                                    dest_ip = new_ip;
                                }
                            }
                        }
                        sig_id = reader.GetUInt32("signature");
                        signature = reader.GetString("sig_name");

                        if (signatureStrings.ContainsKey(sig_id))
                        {
                            signature = signatureStrings[sig_id];
                        }
                        else
                        {
                            signatureStrings.Add(sig_id, signature);
                        }
                        newsid = reader.GetInt32("sid");
                        newcid = reader.GetInt32("cid");
                        list.Add(new Alert()
                        {
                            sid = newsid,
                            cid = newcid,
                            time = reader.GetDateTime("timestamp"),
                            sig_id = sig_id,
                            src_ip = src_ip,
                            dest_ip = dest_ip,
                            protocol = reader.GetInt32("ip_proto"),
                            priority = reader.GetInt32("sig_priority"),
                            subprotocol_dest = reader.GetUInt16("subprotocol"),
                            subprotocol_src = reader.GetUInt16("subprotocol2"),
                            desc = signatureStrings[sig_id]

                        });
                    }
                }
            }

            return list;
        }

        public static string ResolveIP4(UInt32 ip)
        {
            byte[] bytes = BitConverter.GetBytes(ip);
            Array.Reverse(bytes);
            return new IPAddress(bytes).ToString();
        }

        public static string ResolveIP(Byte[] bytes)
        {
            return new IPAddress(bytes).ToString();
        }

    }
}
