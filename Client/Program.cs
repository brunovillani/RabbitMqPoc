using ClientInterface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Client";

            List<MyObject> myobjects = new List<MyObject>()
            {
                new MyObject(){ id = 1, name = "joe" },
                new MyObject(){ id = 2, name = "james" },
                new MyObject(){ id = 3, name = "michael" },
                new MyObject(){ id = 4, name = "max" },
                new MyObject(){ id = 5, name = "tim" },
                new MyObject(){ id = 6, name = "carl" },
                new MyObject(){ id = 7, name = "sam" },
                new MyObject(){ id = 8, name = "tony" },
                new MyObject(){ id = 9, name = "mario" },
                new MyObject(){ id = 10, name = "luigi" },
            };

            foreach (var obj in myobjects)
            {
                Console.WriteLine($"thread {obj.name} sending message...");
                Task.Factory.StartNew(() => {
                    Connector.SendMessage(obj, (changedObject) =>
                    {
                        Console.WriteLine($"thread {obj.id} received {((MyObject)changedObject).id} - {((MyObject)changedObject).name}");
                    });
                });
            }

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}
