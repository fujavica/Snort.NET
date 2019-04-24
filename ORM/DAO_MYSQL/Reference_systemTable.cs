using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using snortdb;

namespace razor.ORM.DAO_MYSQL
{
    public class Reference_systemTable
    {
        public static Dictionary<int, string> GetRefClasses(MySqlConnection conn)
        {
            Dictionary<int, string> refs = new Dictionary<int, string>();

            using (conn)
            {
                conn.Open();
                MySqlCommand cmd;

                cmd = new MySqlCommand("SELECT * FROM reference_system", conn);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    int ref_system_id;
                    string url;
                    while (reader.Read())
                    {
                        ref_system_id = reader.GetInt32("ref_system_id");
                        try
                        {
                            url = reader.GetString("url");
                        }
                        catch (Exception)
                        {
                            url = null;
                        }

                        refs.Add(ref_system_id, url);
                    }
                }
            }
            return refs;
        }
    }
}
