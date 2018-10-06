using System;

namespace DatabaseHandler
{
    class Connection : DatabaseConnectionInterface
    {
        private string hostName;
        private string userName;
        private string password;

        public Connection(
            string hostName,
            string userName,
            string password
        ) {
            this.hostName = hostName;
            this.userName = userName;
            this.password = password;
        }

        public string[] select()
        {
            return new string[] {"TODO", "create", "database", "handler"};
        }

        public void insert()
        {
        }
    }
}
