using System;
using RabbitMQ.Client;
using System.Text;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using System.Collections.Generic;

using System.Threading;
using Db = Database.Connection;
using Rmq = MessageBroker.Connection;

namespace Consumer
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
            string queueName = "analytics_test_queue";
            string binding = "analytics.#";

            using(var connection = factory.CreateConnection())
            {
                using(var channel = connection.CreateModel())
                {
                    Console.WriteLine(" [*] Check that exchange exists -> {0}", exchangeName);
                    channel.ExchangeDeclare(
                        exchange: exchangeName,
                        type: "topic"
                    );

                    Console.WriteLine(" [*] Check that queue exists -> {0}", queueName);
                    channel.QueueDeclare(
                        queue: queueName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null
                    );

                    Console.WriteLine(" [*] Check that binding exists -> {0}", binding);
                    channel.QueueBind(
                        queue: queueName,
                        exchange: exchangeName,
                        routingKey: binding
                    );

                    Console.WriteLine(" [*] Ensure limit for messages prefetching");
                    channel.BasicQos(
                        prefetchSize: 0,
                        prefetchCount: 1,
                        global: false
                    );

                    Console.WriteLine(" [*] Waiting for messages. To exit press [CTRL+C]");
                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);
                        var routingKey = ea.RoutingKey;
                        Console.WriteLine(" [x] Received '{0}':'{1}'", routingKey, message);

                        Console.WriteLine(" [x] Processing...");
                        Thread.Sleep(5000);

                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                        Console.WriteLine(" [x] Processed '{0}':'{1}'", routingKey, message);
                    };
                    channel.BasicConsume(
                        queue: queueName,
                        consumer: consumer
                    );

                    Console.WriteLine(" Press [enter] to exit.");
                    Console.ReadLine();
                }
            }
        }
    }
}
