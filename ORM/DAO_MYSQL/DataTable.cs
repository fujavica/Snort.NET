using MySql.Data.MySqlClient;
using System;
using snortdb;

namespace razor.ORM.DAO_MYSQL
{
    public class DataTable
    {
        public static string GetData(int cid, int sid, MySqlConnection conn)
        {
            string data = null;

            using (conn)
            {
                conn.Open();
                MySqlCommand cmd;

                cmd = new MySqlCommand("SELECT * FROM data WHERE cid = " + cid.ToString() + " AND sid = " + sid.ToString(), conn);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        data = reader.GetString("data_payload");
                    }
                }
            }
            return data;
        }
    }
}
