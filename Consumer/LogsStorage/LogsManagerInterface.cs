using System;

namespace LogsStorage
{
    interface LogsManagerInterface
    {
        bool logException(Exception exception, string message = null);
        bool logMessage(string message);
    }
}
