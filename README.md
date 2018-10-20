# cSharpConsumer
sample c# consumer for bi system

### Dev usage
- install [virtualbox](https://www.virtualbox.org) and [vagrant](https://www.vagrantup.com/) 
- clone repo and run `vagrant up` in projects directory - this will pull images and build an environment
- restart machine -> run `vagrant halt` and then `vagrant up` -> or run `vagrant reload`
- you can ssh to machine using `vagrant ssh`
- files where Vagrantfile lives are mapped to `/vagrant` directory inside machine
- you can launch rabbitmq ui in your browser at `localhost:15672` - username is test and password is test
- ssh to machine and go to `/vagrant/Consumer` directory inside virtual machine and start the consumer
```
vagrant ssh
cd /vagrant/Consumer
dotnet run
```
- ssh to machine and go to `/vagrant/Producer` directory inside virtual machine and try to publish message
```
vagrant ssh
cd /vagrant/Producer
dotnet run
```
- follow instructions to publish message for instance to publish 5 messages with routing key `analytics.Sessions` with payload taken from `sessions.json` file run
```
dotnet run 5 sessions
```
- if message has routing key `analytics.Sessions` consumer will try to use class Sessions under BiStorage namespace for handling
- you can find event handlers in `Consumer/BiStorage/EventHandlers` directory
- remember to clean builds after changes in code before running `dotnet run`
```
dotnet clean
```

### Used packages
For this to work following packages were included in Consumer and Producer
```
dotnet add package RabbitMQ.Client
dotnet add package Newtonsoft.Json
dotnet add package System.Data.SqlClient
```

### Sql from inside machine
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
CREATE DATABASE TestDB
GO
SELECT Name from sys.Databases
GO
```
Use `TestDB` database and create `Inventory` table and insert sample row
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
Drop databse
```
DROP DATABASE TestDB
GO
```
Exit
```
QUIT
```
You can execute sql file in db using
```
/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P 'MSSQLadmin!' < somefile.sql
```
