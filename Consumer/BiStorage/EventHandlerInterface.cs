using LM = LogsStorage.LogsManagerInterface;
using DB = DatabaseHandler.DatabaseConnectionInterface;

namespace BiStorage
{
    interface EventHandlerInterface
    {
        DB db();
        LM logger();
        bool process(string jsonMessage);
        string getDefault();

        // for dev purposes
        string getRandom(int length, bool numbersOnly);
    }
}
