using System;
using System.Net.Sockets;

namespace Server.ClientModel
{
    public class ServerClient : Client
    {
        public TcpClient TcpClient { get; }

        public ServerClient(TcpClient client, Guid id, string login)
        {
            TcpClient = client ?? throw new ArgumentNullException(nameof(client));
            Login = login ?? throw new ArgumentNullException(nameof(login));
            Id = id;
        }

        public ServerClient()
        {
            
        }
    }
}