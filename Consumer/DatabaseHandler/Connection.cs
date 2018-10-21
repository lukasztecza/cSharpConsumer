using System;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace DatabaseHandler
{
    class Connection : DatabaseConnectionInterface
    {
        private string hostName;
        private int port;
        private string dataBase;
        private string userName;
        private string password;

        public Connection(
            string hostName,
            int port,
            string dataBase,
            string userName,
            string password
        ) {
            this.hostName = hostName;
            this.port = port;
            this.dataBase = dataBase;
            this.userName = userName;
            this.password = password;
        }

        public List<Dictionary<string, string>> fetch(string query, Dictionary<string, string> arguments = null)
        {

            using (SqlConnection conn = new SqlConnection(this.getConnectionString()))
            {
                List<Dictionary<string, string>> results = new List<Dictionary<string, string>>();
                conn.Open();
                SqlCommand command = createSqlCommand(conn, query, arguments);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int rowLength = reader.FieldCount;
                        Dictionary<string, string> values = new Dictionary<string, string>();
                        for (int i = 0; i < rowLength; i++) {
                            values.Add(reader.GetName(i), reader.GetValue(i).ToString());
                        }
                        results.Add(values);
                    }
                }

                return results;
            }
        }

        public int execute(string query, Dictionary<string, string> arguments = null)
        {
            using (SqlConnection conn = new SqlConnection(this.getConnectionString()))
            {
                conn.Open();
                SqlCommand command = createSqlCommand(conn, query, arguments);

                return command.ExecuteNonQuery();
            }
        }

        private SqlCommand createSqlCommand(SqlConnection conn, string query, Dictionary<string, string> arguments = null)
        {
            SqlCommand command = new SqlCommand(query, conn);
            foreach (KeyValuePair<string, string> entry in arguments)
            {
                command.Parameters.Add(new SqlParameter(entry.Key, entry.Value));
            }

            return command;
        }

        private string getConnectionString()
        {
            return "Server=" + this.hostName + ", " + this.port.ToString() + ";Database=" + this.dataBase +  ";User Id=" + this.userName + ";Password=" + this.password + ";";
        }
    }
}
