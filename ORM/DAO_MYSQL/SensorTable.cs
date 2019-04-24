using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using snortdb;

namespace razor.ORM.DAO_MYSQL
{
    public class SensorTable
    {
        public static List<Sensor> GetSensors(MySqlConnection conn)
        {
            List<Sensor> sensors = new List<Sensor>();

            using (conn)
            {
                conn.Open();
                MySqlCommand cmd;

                cmd = new MySqlCommand("SELECT sid, hostname, interface, filter, last_cid FROM sensor", conn);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Sensor s = new Sensor();
                        s.sid = reader.GetInt32("sid");
                        if (!reader.IsDBNull(1))
                        {
                            s.hostname = reader.GetString("hostname");
                        }
                        if (!reader.IsDBNull(2))
                        {
                            s.iface = reader.GetString("interface");
                        }
                        if (!reader.IsDBNull(3))
                        {
                            s.filter = reader.GetString("filter");
                        }

                        s.last_cid = reader.GetInt32("last_cid");
                        sensors.Add(s);
                    }
                }
            }
            return sensors;
        }
    }
}
