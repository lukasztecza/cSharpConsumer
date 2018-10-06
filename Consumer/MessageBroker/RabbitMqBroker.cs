using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using BiEventHandler;

using System.Threading;

namespace MessageBroker
{
    class RabbitMqBroker : MessageBrokerInterface
    {
        private EventProcessor eventProcessor;
        private string hostName;
        private string userName;
        private string password;
        private string exchangeName;
        private string queueName;
        private string binding;
        private ConnectionFactory factory;

        public RabbitMqBroker(
            EventProcessor eventProcessor,
            string hostName,
            string userName,
            string password,
            string exchangeName,
            string queueName,
            string binding
        ) {
            this.eventProcessor = eventProcessor;
            this.hostName = hostName;
            this.userName = userName;
            this.password = password;
            this.exchangeName = exchangeName;
            this.queueName = queueName;
            this.binding = binding;
            this.factory = new ConnectionFactory();
        }

        public void connectAndListen()
        {
            using(var connection = this.factory.CreateConnection())
            {
                using(var channel = connection.CreateModel())
                {
                    Console.WriteLine(" [*] Check that exchange exists -> {0}", this.exchangeName);
                    channel.ExchangeDeclare(
                        exchange: this.exchangeName,
                        type: "topic"
                    );

                    Console.WriteLine(" [*] Check that queue exists -> {0}", this.queueName);
                    channel.QueueDeclare(
                        queue: this.queueName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null
                    );

                    Console.WriteLine(" [*] Check that binding exists -> {0}", this.binding);
                    channel.QueueBind(
                        queue: this.queueName,
                        exchange: this.exchangeName,
                        routingKey: this.binding
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
                        this.eventProcessor.handle(routingKey, message);
                        Thread.Sleep(5000);

                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                        Console.WriteLine(" [x] Processed '{0}':'{1}'", routingKey, message);
                    };
                    channel.BasicConsume(
                        queue: this.queueName,
                        consumer: consumer
                    );

                    Console.WriteLine(" Press [enter] to exit.");
                    Console.ReadLine();
                }
            }
        }
    }
}
