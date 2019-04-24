using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using snortdb;

namespace razor.ORM.DAO_MYSQL
{
    public class ReferenceTable
    {
        public static List<snortdb.Ref> GetReference(UInt32 sig_id, MySqlConnection conn)
        {
            List<snortdb.Ref> sigrefs = new List<snortdb.Ref>();
            List<int> ref_ids = new List<int>();

            using (conn)
            {
                conn.Open();
                MySqlCommand cmd;

                cmd = new MySqlCommand("SELECT ref_id FROM sig_reference WHERE sig_id= " + sig_id.ToString(), conn);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ref_ids.Add(reader.GetInt32("ref_id"));
                    }
                }
                if (ref_ids.Count() == 0) return null;

                foreach (int ref_id in ref_ids)
                {
                    cmd = new MySqlCommand("SELECT * FROM reference WHERE ref_id = " + ref_id.ToString(), conn);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        snortdb.Ref sigref = new snortdb.Ref();
                        while (reader.Read())
                        {
                            sigref.ref_system_id = reader.GetInt32("ref_system_id");
                            sigref.ref_tag = reader.GetString("ref_tag");
                            sigref.ref_id = ref_id;
                            sigrefs.Add(sigref);
                        }
                    }
                }
            }
            return sigrefs;
        }
    }
}
