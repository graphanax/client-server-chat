using System;

namespace Server.ClientModel
{
    [Serializable]
    public class EncryptedMessage
    {
        public EncryptedMessage(Message message, ConnectedClient client)
        {
            Message = message;
            Client = client;
        }

        public Message Message { get; }
        public ConnectedClient Client { get; }
        
        public MessageItem EncMessageToMessageItem(string text)
        {
            return new MessageItem
            {
                Content = text,
                Login = Client.Login,
                SendTime = Message.SendTime
            };
        }
    }
    
    [Serializable]
    public class Message
    {
        public Message(string[] content, DateTime sendTime)
        {
            Content = content;
            SendTime = sendTime;
        }

        public string[] Content { get; set; }
        public DateTime SendTime { get; set; }
    }
    
    public class MessageItem
    {
        public string Content { get; set; }
        public DateTime SendTime { get; set; }
        public string Login { get; set; }
    }
}