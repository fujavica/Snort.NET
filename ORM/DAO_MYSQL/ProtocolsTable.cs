using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using snortdb;

namespace razor.ORM.DAO_MYSQL
{
    public class ProtocolsTable
    {
        public static List<Protocol> GetProtocols(MySqlConnection conn)
        {
            List<Protocol> protocols = new List<Protocol>();

            using (conn)
            {
                conn.Open();
                MySqlCommand cmd;

                cmd = new MySqlCommand("SELECT * FROM protocols", conn);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        int id = reader.GetInt32("pid");
                        string name = reader.GetString("protocol");
                        string reference = reader.GetString("reference");
                        protocols.Add(new Protocol(id, name, reference));
                    }
                }
            }
            return protocols;
        }
    }
}
