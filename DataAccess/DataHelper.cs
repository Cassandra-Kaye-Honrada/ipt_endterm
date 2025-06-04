using MySql.Data.MySqlClient;
using System.Data;

namespace Endterm_IPT.DataAccess
{
    public class DataHelper
    {

        private readonly string _connString;
        public DataHelper(string connString)
        {
            _connString = connString;
        }

        public DataTable selectQuery(string query) 
        {
            DataTable dt = new DataTable();
            using (MySqlConnection conn = new MySqlConnection(_connString))
            {
                conn.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }
                return dt;
        }

        public int executeQuery(string query)
        {
            using (MySqlConnection conn = new MySqlConnection(_connString))
            {
                conn.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    return cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
