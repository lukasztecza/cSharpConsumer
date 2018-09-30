# cSharpConsumer
Sample c# consumer

### Usage
- clone repo and run vagrant up
- ssh to machine and go to `/vagrant`
- launch rabbitmq ui at `localhost:15672`
- go to Consumer directory and run `dotnet run` -> it will create exchange and a queue and listen for messages
- go to Producer directory and run `dotnet run` -> follow instructions to publish message

### Tips
For this to work you need to run in both apps the following
```
dotnet add package RabbitMQ.Client
dotnet add package Newtonsoft.Json
```
Remember to run after changes before `dotnet run` as some caching magic happens
```
dotnet clean
```
