# cSharpConsumer
Sample c# consumer dev playground

### Usage
- clone repo and run vagrant up
- ssh to machine and go to `/vagrant`
- launch rabbitmq ui in your browser at `localhost:15672`
- go to Consumer directory and run `dotnet run` -> it will create exchange and a queue and listen for messages
- go to Producer directory and run `dotnet run` -> follow instructions to publish message

### Tips
For this to work following packages were included in both Consumer and Producer
```
dotnet add package RabbitMQ.Client
dotnet add package Newtonsoft.Json
```
Remember to run after changes before `dotnet run` as some caching magic happens
```
dotnet clean
```

### Sql
Show sql server status
```
systemctl status mssql-server
```
Run sql client
```
/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P 'MSSQLadmin!'
```
* you can run below to be able to run sqlcmd without absolute path
```
echo 'export PATH="$PATH:/opt/mssql-tools/bin"' >> ~/.bashrc
source ~/.bashrc
```
Show databases
```
SELECT Name from sys.Databases
GO
```
Use `test_db` database and create `Inventory` table and insert sample row
```
USE TestDB;
CREATE TABLE Inventory (id INT, name NVARCHAR(50), quantity INT);
INSERT INTO Inventory VALUES (1, 'banana', 150); INSERT INTO Inventory VALUES (2, 'orange', 154);
GO
```
Show tables
```
SELECT Distinct TABLE_NAME FROM information_schema.TABLES
GO
```
Select from table
```
SELECT * FROM Inventory WHERE quantity > 152;
GO
```
Exit
```
QUIT
```
You can execute sql file in db using
```
/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P 'MSSQLadmin!' < db.sql
```
