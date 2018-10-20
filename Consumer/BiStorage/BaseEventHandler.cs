using System;
using LM = LogsStorage.LogsManagerInterface;
using DB = DatabaseHandler.DatabaseConnectionInterface;

namespace BiStorage
{
    abstract class BaseEventHandler : EventHandlerInterface
    {
        private LM logsManager;
        private DB databaseHandler;

        public BaseEventHandler(
            LM logsManager,
            DB databaseHandler
        ) {
            this.logsManager = logsManager;
            this.databaseHandler = databaseHandler;
        }

        public DB db()
        {
            return this.databaseHandler;
        }

        public LM logger()
        {
            return this.logsManager;
        }

        public string getDefault()
        {
            return "UNIDENTIFIED";
        }

        public string getRandom(int length, bool numbersOnly = false)
        {
            string chars = numbersOnly ? "0123456789" : "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            char[] stringChars = new char[length];
            Random random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new String(stringChars);
        }

        abstract public bool process(string jsonMessage);
    }
}
