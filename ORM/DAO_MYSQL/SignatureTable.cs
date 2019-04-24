using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using snortdb;

namespace razor.ORM.DAO_MYSQL
{
    public class SignatureTable
    {

        public static Dictionary<UInt32, string> GetAll(MySqlConnection conn)
        {
            Dictionary<UInt32,string> signatures = new Dictionary<UInt32, string>();
            using (conn)
            {
                conn.Open();
                MySqlCommand cmd;
                cmd = new MySqlCommand("SELECT sig_id, sig_name FROM signature", conn);
                UInt32 sigid;
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        sigid = reader.GetUInt32("sig_id");
                        if (!signatures.ContainsKey(sigid))
                        { 
                            signatures.Add(sigid, reader.GetString("sig_name"));
                        }
                    }
                }
            }
            return signatures;
        }
        public static Signature GetSignature(int signature_id, MySqlConnection conn)
        {
            Signature sig = new Signature();

            using (conn)
            {
                conn.Open();
                MySqlCommand cmd;

                cmd = new MySqlCommand("SELECT * FROM signature WHERE sig_id = " + signature_id.ToString(), conn);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        sig.sig_id = reader.GetUInt32("sig_id");
                        sig.sig_class_id = reader.GetInt32("sig_class_id");
                        sig.sig_name = reader.GetString("sig_name");
                        sig.sig_priority = reader.GetInt32("sig_priority");
                        sig.sig_rev = reader.GetInt32("sig_rev");
                        sig.sig_sid = reader.GetInt32("sig_sid");
                        sig.sig_gid = reader.GetInt32("sig_gid");
                    }
                }
            }
            return sig;
        }
    }
}
