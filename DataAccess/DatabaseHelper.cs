using MySql.Data.MySqlClient;
using System.Data;

namespace Endterm_IPT.DataAccess
{
    public class DatabaseHelper
    {
        private readonly string _connString;
        public DatabaseHelper(string connString)
        {
            _connString = connString;
        }

        public DataTable SelectQuery(string query)
        {
            DataTable dataTable = new DataTable();
            using (MySqlConnection connection = new MySqlConnection(_connString))
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }
            return dataTable;
        }

        public int ExecuteQuery(string query)
        {
            using (MySqlConnection connection = new MySqlConnection(_connString))
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    return command.ExecuteNonQuery();
                }
            }
        }
    }
}

