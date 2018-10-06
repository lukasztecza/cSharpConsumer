using System;
using DB = DatabaseHandler.Connection;
using BiEventHandler;
using MB = MessageBroker.RabbitMqBroker;

namespace BiConsumer
{
    class Program
    {
        public static void Main(string[] args)
        {
            var databaseHandler = new DB(
                "localhost",
                "SA",
                "MSSQLadmin!"
            );

            var eventProcessor = new EventProcessor(
                databaseHandler
            );

            var messageBroker = new MB(
                eventProcessor,
                "localhost",               //host
                "test",                    //user
                "test",                    //password
                "analytics_test_exchange", //exchange name
                "analytics_test_queue",    //queue name
                "analytics.#"              //binding
            );

            messageBroker.connectAndListen();
        }
    }
}
