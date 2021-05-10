using System;

namespace Server.ClientModel
{
    [Serializable]
    public sealed class ConnectedClient : Client
    {
        public ConnectedClient(string host, string login)
        {
            Host = host ?? throw new ArgumentNullException(nameof(host));
            Login = login ?? throw new ArgumentNullException(nameof(login));
            IsOnline = true;
        }

        public string Host { get; }
        public bool IsOnline;
    }
}