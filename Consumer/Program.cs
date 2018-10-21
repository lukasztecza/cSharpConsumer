using System;
using Newtonsoft.Json;
using DB = DatabaseHandler.Connection;
using LM = LogsStorage.LogsManager;
using BM = BiStorage.BiManager;
using MB = MessageBroker.RabbitMqBroker;

namespace BiConsumer
{
    class Program
    {
        public static void Main(string[] args)
        {
            string parametersFileName = "parameters.json";
            if (System.IO.File.Exists(parametersFileName)) {
                string jsonParameters = System.IO.File.ReadAllText(parametersFileName);
                dynamic parameters = JsonConvert.DeserializeObject<dynamic>(jsonParameters);

                DB databaseHandler = new DB(
                    parameters.databaseHost.ToString(),
                    parameters.databasePort.ToObject<int>(),
                    parameters.databaseName.ToString(),
                    parameters.databaseUser.ToString(),
                    parameters.databasePassword.ToString()
                );

                LM logsManager = new LM(
                    parameters.logStack.ToObject<bool>()
                );

                BM biManager = new BM(
                    logsManager,
                    databaseHandler
                );

                MB messageBroker = new MB(
                    logsManager,
                    biManager,
                    parameters.messageBrokerHost.ToString(),
                    parameters.messageBrokerPort.ToObject<int>(),
                    parameters.messageBrokerVHost.ToString(),
                    parameters.messageBrokerUser.ToString(),
                    parameters.messageBrokerPassword.ToString(),
                    parameters.messageBrokerExchangeName.ToString(),
                    parameters.messageBrokerQueueName.ToString(),
                    parameters.messageBrokerBinding.ToString(),
                    parameters.messageBrokerMaxRetries.ToObject<int>(),
                    parameters.messageBrokerFailMessageHoldingTime.ToObject<int>()
                );

                messageBroker.connectAndListen();
            } else {
                Console.WriteLine("Create {0} with parameters for database connection and message broker connection -> look in Manifest directory for samples", parametersFileName);
            }
        }
    }
}
