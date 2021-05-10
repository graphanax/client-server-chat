using System;

namespace Server.ClientModel
{
    [Serializable]
    public abstract class Client
    {
        public string Login { get; protected set; }
    }
}