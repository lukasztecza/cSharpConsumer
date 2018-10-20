using System;
using LM = LogsStorage.LogsManagerInterface;
using DB = DatabaseHandler.DatabaseConnectionInterface;
using System.Text.RegularExpressions;
using System.Reflection;

namespace BiStorage
{
    class BiManager
    {
        private LM logsManager;
        private DB databaseHandler;

        public BiManager(
             LM logsManager,
             DB databaseHandler
        ) {
            this.logsManager = logsManager;
            this.databaseHandler = databaseHandler;
        }

        public bool handle(string routingKey, string jsonMessage)
        {
            bool result = false;
            string className = "BiStorage." + Regex.Replace(routingKey.Replace("analytics.", ""), @"[^a-zA-Z0-9]", "");
            if (Type.GetType(className) == null) {
                this.logsManager.logMessage(string.Format(
                    "No eventHandler class has been found for routing key -> {0} -> ignoring", routingKey
                ));

                return true;
            }

            try {
                EventHandlerInterface selectedHandler = Assembly.GetExecutingAssembly().CreateInstance(
                    className,
                    false,
                    BindingFlags.Default,
                    null,
                    new object[] {this.logsManager, this.databaseHandler},
                    null,
                    null
                ) as EventHandlerInterface;
                result = selectedHandler.process(jsonMessage);
            } catch(Exception exception) {
                this.logsManager.logException(exception, string.Format(
                    "Failed to process event with routing key -> {0} -> due to exception", routingKey
                ));

                return false;
            }

            if (result == false) {
                this.logsManager.logMessage(string.Format("Could not process event with routing key -> {0}", routingKey));
            }

            return result;
        }
    }
}
