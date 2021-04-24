using System;

namespace Server.ClientModel
{
    [Serializable]
    public sealed class ConnectedClient : Client
    {
        public ConnectedClient(string source, string login)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Login = login ?? throw new ArgumentNullException(nameof(login));
        }

        public string Source { get; }
    }
}