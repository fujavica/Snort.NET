using MySql.Data.MySqlClient;
using System;
using snortdb;

namespace razor.ORM.DAO_MYSQL
{
    public class SnortContext
    {
        public string ConnectionString { get; set; }

        public SnortContext(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString);
        }

    }

}