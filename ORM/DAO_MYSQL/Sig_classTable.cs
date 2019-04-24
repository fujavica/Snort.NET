using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using snortdb;

namespace razor.ORM.DAO_MYSQL
{
    public class Sig_classTable
    {
        public static Dictionary<int, string> GetClassNames(MySqlConnection conn)
        {

            Dictionary<int, string> classNames = new Dictionary<int, string>();

            using (conn)
            {
                conn.Open();
                MySqlCommand cmd;

                cmd = new MySqlCommand("SELECT * FROM sig_class", conn);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    int id;
                    string name;
                    while (reader.Read())
                    {
                        id = reader.GetInt32("sig_class_id");
                        name = reader.GetString("sig_class_name");
                        classNames.Add(id, name);
                    }
                }
            }
            return classNames;
        }
    }
}
