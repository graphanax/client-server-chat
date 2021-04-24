using System;

namespace Server.ClientModel
{
    [Serializable]
    public abstract class Client
    {
        public int Id { get; protected set; }
        public string Login { get; protected set; }
    }
}