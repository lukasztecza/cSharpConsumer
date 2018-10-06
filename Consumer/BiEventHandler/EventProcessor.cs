using System;
using DB = DatabaseHandler.Connection;
using Newtonsoft.Json;

namespace BiEventHandler
{
    class EventProcessor
    {
        private DB databaseHandler;

        public EventProcessor(
             DB databaseHandler
        ) {
            this.databaseHandler = databaseHandler;
        }

        public bool handle(string routingKey, string jsonMessage)
        {
            Console.WriteLine(" [x] I will use Newtonsoft.Json to parse it");
            string[] result = this.databaseHandler.select();
            Console.WriteLine(result[2]);
            return true;
        }
    }
}
