namespace DatabaseHandler
{
    interface DatabaseConnectionInterface
    {
        string[] select();
        void insert();
    }
}
