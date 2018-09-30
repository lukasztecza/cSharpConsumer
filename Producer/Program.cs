using System;
using System.Linq;
using RabbitMQ.Client;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Producer
{
    class Program
    {
        public static void Main(string[] args)
        {
            var factory = new ConnectionFactory();
            factory.HostName = "localhost";
            factory.UserName = "test";
            factory.Password = "test";
            string exchangeName = "analytics_test_exchange";

            using(var connection = factory.CreateConnection()) 
            {
                using(var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(
                        exchange: exchangeName,
                        type: "topic"
                    );

                    if (args.Length < 2) {
                        Console.WriteLine("Need at least 2 arguments: routing_key message");

                        return;
                    }

                    var routingKey = args[0];
                    var message = getMessage(args);

                    var body = Encoding.UTF8.GetBytes(message);
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

        private static string getMessage(string [] args)
        {
            var arr = args.Skip(1).ToArray();
            Dictionary<string, string> payload = new Dictionary<string, string>();
            for (int i = 0; i < arr.Length - 1; i += 2)
            {
                payload.Add(arr[i], arr[i + 1]);
            }

            return JsonConvert.SerializeObject(payload);
        }
    }
}
