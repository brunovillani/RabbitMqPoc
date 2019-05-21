using Newtonsoft.Json;
using System;

namespace ClientInterface
{
    public class MessageCapsule
    {
        private string id;
        public string Id
        {
            get { if (id == null) { id = Guid.NewGuid().ToString(); } return id; }
            set { id = value; }
        }
        public string Message;

        public MessageCapsule() { }
        public MessageCapsule(string message)
        {
            this.Message = message;
        }

        public MessageCapsule(object obj)
        {
            this.Message = JsonConvert.SerializeObject(obj);
        }

        public void SetMessage(object obj)
        {
            this.Message = JsonConvert.SerializeObject(obj);
        }
    }

    public class MyObject
    {
        public int id;
        public string name;
    }
}
