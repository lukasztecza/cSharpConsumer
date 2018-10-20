using System;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text.RegularExpressions;

namespace Producer
{
    class Program
    {
        public static void Main(string[] args)
        {
            string parametersFileName = "../Consumer/parameters.json";
            if (System.IO.File.Exists(parametersFileName)) {
                string jsonParameters = System.IO.File.ReadAllText(parametersFileName);
                dynamic parameters = JsonConvert.DeserializeObject<dynamic>(jsonParameters);

                ConnectionFactory factory = new ConnectionFactory();
                factory.HostName = parameters.messageBrokerHost.ToString();
                factory.Port = parameters.messageBrokerPort.ToObject<int>();
                factory.VirtualHost = parameters.messageBrokerVHost.ToString();
                factory.UserName = parameters.messageBrokerUser.ToString();
                factory.Password = parameters.messageBrokerPassword.ToString();
                string exchangeName = parameters.messageBrokerExchangeName.ToString();

                using(var connection = factory.CreateConnection())
                {
                    using(var channel = connection.CreateModel())
                    {
                        channel.ExchangeDeclare(
                            exchange: exchangeName,
                            durable: true,
                            type: "topic"
                        );

                        string routingKey = "";
                        string message = "";
                        int messageCount = 0;
                        if (args.Length == 2) {
                            string fileName = Regex.Replace(args[1], @"[^a-zA-Z0-9]", "") + ".json";
                            if (System.IO.File.Exists(fileName)) {
                                routingKey = "analytics." + args[1].First().ToString().ToUpper() + args[1].Substring(1);
                                message = System.IO.File.ReadAllText(fileName);
                                messageCount = Int32.Parse(args[0]);
                            } else {
                                Console.WriteLine("Ensure that {0} file exists and contains message sample", fileName);
                            }
                        } else {
                            Console.WriteLine("Need 2 arguments: messageCount eventName");

                            return;
                        }

                        byte[] body = Encoding.UTF8.GetBytes(message);
                        for (int i = 0; i < messageCount; i++) {
                            channel.BasicPublish(
                                exchange: exchangeName,
                                routingKey: routingKey,
                                basicProperties: null,
                                body: body
                            );
                            Console.WriteLine(" [x] Sent '{0}':'{1}'", routingKey, message);
                        }
                    }
                }
            }
        }
    }
}
