using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ClientInterface
{
    public static class Connector
    {
        private static Dictionary<string, Action<object>> actions;
        static Connector()
        {
            if (actions == null)
                actions = new Dictionary<string, Action<object>>();
        }
        public static void SendMessage(object obj, Action<object> action)
        {
            string messageId = null;
            string messageName = ((MyObject)obj).name;
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "send_queue",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                MessageCapsule capsule = new MessageCapsule(obj);
                messageId = capsule.Id;
                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(capsule));
                Connector.actions.Add(capsule.Id, action);

                channel.BasicPublish(exchange: "",
                                     routingKey: "send_queue",
                                     basicProperties: null,
                                     body: body);

                Console.WriteLine($"sent message {messageName}...");
            }

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "receive_queue",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    MessageCapsule capsule = JsonConvert.DeserializeObject<MessageCapsule>(Encoding.UTF8.GetString(ea.Body));
                    MyObject capsuleObj = JsonConvert.DeserializeObject<MyObject>(capsule.Message);
                    Action<object> act = Connector.actions[capsule.Id];
                    act(capsuleObj);
                };

                channel.BasicConsume(queue: "receive_queue",
                                     autoAck: true,
                                     consumer: consumer);
                Console.WriteLine($"configure receive - {messageName}...");

                int checkTimes = 10;
                while (Connector.actions.ContainsKey(messageId) && checkTimes > 0)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(5).Milliseconds);
                    checkTimes--;
                }
                Console.WriteLine($"closing thread {messageId}...");
            }
        }
    }
}
