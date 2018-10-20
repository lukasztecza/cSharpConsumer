using System.Collections.Generic;

namespace DatabaseHandler
{
    interface DatabaseConnectionInterface
    {
        List<Dictionary<string, string>> fetch(string query, Dictionary<string, string> arguments = null);
        int execute(string query, Dictionary<string, string> arguments = null);
    }
}
