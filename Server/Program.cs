using ClientInterface;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Server";
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "send_queue",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    MessageCapsule capsule = JsonConvert.DeserializeObject<MessageCapsule>(Encoding.UTF8.GetString(ea.Body));
                    MyObject capsuleObj = JsonConvert.DeserializeObject<MyObject>(capsule.Message);
                    Console.WriteLine($"received {capsule.Id} - {capsuleObj.name}");

                    capsuleObj.name += " changed";

                    capsule.SetMessage(capsuleObj);
                    SendMessage(factory, capsule);
                };
                channel.BasicConsume(queue: "send_queue",
                                     autoAck: true,
                                     consumer: consumer);

                Console.WriteLine($"configured receiving...");
                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }

        private static void SendMessage(ConnectionFactory factory, MessageCapsule capsule)
        {
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "receive_queue",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(capsule));

                channel.BasicPublish(exchange: "",
                                     routingKey: "receive_queue",
                                     basicProperties: null,
                                     body: body);
            }
        }
    }
}
