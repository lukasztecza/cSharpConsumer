using System;

namespace LogsStorage
{
    class LogsManager : LogsManagerInterface
    {
        private bool logStack;

        public LogsManager(
             bool logStack
        ) {
            this.logStack = logStack;
        }

        public bool logException(Exception exception, string message = null)
        {
            if (message != null) {
                Console.WriteLine(message + ":");
            }
            Console.WriteLine("Message -> {0}", exception.Message);
            Console.WriteLine("Source -> {0}", exception.Source);
            if (this.logStack == true) {
                this.logStackTrace(exception);
            }

            return true;
        }

        public bool logMessage(string message)
        {
            Console.WriteLine(message);

            return true;
        }

        private void logStackTrace(Exception exception)
        {
            Console.WriteLine("StackTrace -> {0}", exception.StackTrace);
        }
    }
}
