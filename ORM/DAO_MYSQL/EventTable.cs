using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using snortdb;

namespace razor.ORM.DAO_MYSQL
{
    public class EventTable
    {

        public static List<Event> EatEvents(int limit, MySqlConnection conn)
        {
            List<Event> list = new List<Event>();

            using (conn)
            {
                conn.Open();

                MySqlCommand cmd;

                if (limit == 0)
                {
                    cmd = new MySqlCommand("SELECT * FROM event", conn);
                }
                else
                {
                    cmd = new MySqlCommand("SELECT * FROM event LIMIT " + limit.ToString(), conn);
                }
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Event()
                        {
                            sid = reader.GetInt32("sid"),
                            cid = reader.GetInt32("cid"),
                            signature = reader.GetInt32("signature"),
                            timestamp = reader.GetDateTime("timestamp"),
                        });
                    }
                }
            }
            return list;
        }

        public static List<Event> UpdateEvents(int sid, int start, MySqlConnection conn)
        {
            List<Event> list = new List<Event>();

            using (conn)
            {
                conn.Open();

                MySqlCommand cmd;

                cmd = new MySqlCommand("SELECT * FROM event WHERE sid =" + sid + " AND cid > " + start, conn);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Event()
                        {
                            sid = reader.GetInt32("sid"),
                            cid = reader.GetInt32("cid"),
                            signature = reader.GetInt32("signature"),
                            timestamp = reader.GetDateTime("timestamp"),
                        });
                    }
                }
            }

            return list;
        }

        public static Event GetEvent(int cid, int sid, MySqlConnection conn)

        {
            Event eve = new Event();
            using (conn)
            {
                conn.Open();
                MySqlCommand cmd;

                cmd = new MySqlCommand("SELECT * FROM event WHERE cid = " + cid + " AND sid = " + sid, conn);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        eve.sid = reader.GetInt32("sid");
                        eve.cid = reader.GetInt32("cid");
                        eve.signature = reader.GetInt32("signature");
                        eve.timestamp = reader.GetDateTime("timestamp");
                    }
                }
            }
            return eve;
        }
    }
}
