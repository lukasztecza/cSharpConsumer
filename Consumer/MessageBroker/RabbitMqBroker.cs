using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using LM = LogsStorage.LogsManagerInterface;
using BiStorage;
using Newtonsoft.Json;

namespace MessageBroker
{
    class RabbitMqBroker : MessageBrokerInterface
    {
        private LM logsManager;
        private BiManager biManager;
        private string exchangeName;
        private string queueName;
        private string binding;
        private int maxRetries;
        private int failMessageHoldingTime;
        private ConnectionFactory factory;

        public RabbitMqBroker(
            LM logsManager,
            BiManager biManager,
            string hostName,
            int port,
            string vHostName,
            string userName,
            string password,
            string exchangeName,
            string queueName,
            string binding,
            int maxRetries,
            int failMessageHoldingTime
        ) {
            this.logsManager = logsManager;
            this.biManager = biManager;
            this.exchangeName = exchangeName;
            this.queueName = queueName;
            this.binding = binding;
            this.maxRetries = maxRetries;
            this.failMessageHoldingTime = failMessageHoldingTime;
            this.factory = new ConnectionFactory();
            this.factory.HostName = hostName;
            this.factory.Port = port;
            this.factory.VirtualHost = vHostName;
            this.factory.UserName = userName;
            this.factory.Password = password;
        }

        public void connectAndListen()
        {
            using(var connection = this.factory.CreateConnection())
            {
                using(var channel = connection.CreateModel())
                {
                    string failExchangeName = this.exchangeName + "_fail";
                    string failQueueName = this.queueName + "_fail";
                    string unprocessableQueueName = this.queueName + "_unprocessable";

                    this.logsManager.logMessage(string.Format("Check that exchange exists -> {0}", this.exchangeName));
                    channel.ExchangeDeclare(
                        exchange: this.exchangeName,
                        durable: true,
                        type: "topic"
                    );
                    channel.ExchangeDeclare(
                        exchange: failExchangeName,
                        durable: true,
                        type: "topic"
                    );

                    this.logsManager.logMessage(string.Format("Check that queue exists -> {0}", this.queueName));
                    channel.QueueDeclare(
                        queue: this.queueName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: new Dictionary<string, object> {
                            {"x-dead-letter-exchange", failExchangeName}
                        }
                    );
                    channel.QueueDeclare(
                        queue: failQueueName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: new Dictionary<string, object> {
                            {"x-dead-letter-exchange", this.exchangeName},
                            {"x-message-ttl", this.failMessageHoldingTime}
                        }
                    );
                    channel.QueueDeclare(
                        queue: unprocessableQueueName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null
                    );

                    this.logsManager.logMessage(string.Format("Check that binding exists -> {0}", this.binding));
                    channel.QueueBind(
                        queue: this.queueName,
                        exchange: this.exchangeName,
                        routingKey: this.binding
                    );
                    channel.QueueBind(
                        queue: failQueueName,
                        exchange: failExchangeName,
                        routingKey: this.binding
                    );
                    channel.QueueBind(
                        queue: unprocessableQueueName,
                        exchange: failExchangeName,
                        routingKey: unprocessableQueueName
                    );

                    this.logsManager.logMessage("Ensure limit for messages prefetching");
                    channel.BasicQos(
                        prefetchSize: 0,
                        prefetchCount: 1,
                        global: false
                    );

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body;
                        string message = Encoding.UTF8.GetString(body);
                        string routingKey = ea.RoutingKey;
                        this.logsManager.logMessage(string.Format("Received message with routing key -> {0}", routingKey));
                        bool result = this.biManager.handle(routingKey, message);
                        if (result == false) {
                            this.logsManager.logMessage(string.Format("Could not process message with routing key -> {0}", routingKey));
                            int retries = 0;
                            if (ea.BasicProperties.Headers != null) {
                                foreach (KeyValuePair<string, object> entry in ea.BasicProperties.Headers)
                                {
                                    IEnumerable enumerable = entry.Value as IEnumerable;
                                    if (enumerable != null) {
                                        foreach (Dictionary<string, object> obj in enumerable) {
                                            int currentCount = int.Parse(obj["count"].ToString());
                                            if (retries < currentCount) {
                                                retries = currentCount;
                                            }
                                        }
                                    }
                                }
                            }
                            if (retries >= this.maxRetries) {
                                string stringBody = System.Text.Encoding.UTF8.GetString(body);
                                Dictionary<string, string> unprocessableMessage = new Dictionary<string, string>();
                                DateTimeOffset dateTime = DateTimeOffset.Now;
                                unprocessableMessage.Add("originalBody", stringBody);
                                unprocessableMessage.Add("originalRoutingKey", routingKey);
                                unprocessableMessage.Add("retries", retries.ToString());
                                unprocessableMessage.Add("dateTime", dateTime.ToString("yyyy-MM-dd HH:mm:ss K"));
                                byte[] unprocessableBody = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(unprocessableMessage));
                                channel.BasicPublish(
                                    exchange: failExchangeName,
                                    routingKey: unprocessableQueueName,
                                    basicProperties: null,
                                    body: unprocessableBody
                                );
                                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                            } else {
                                channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                            }
                        } else {
                            channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                            this.logsManager.logMessage(string.Format("Processed message with routing key -> {0}", routingKey));
                        }
                    };
                    channel.BasicConsume(
                        queue: this.queueName,
                        autoAck: false,
                        consumer: consumer
                    );
                    this.logsManager.logMessage("Waiting for messages. To exit press [CTRL+C] or [enter]");
                    Console.ReadLine();
                }
            }
        }
    }
}
