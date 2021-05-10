using System;
using System.Net.Sockets;

namespace Server.ClientModel
{
    public class ServerClient : Client
    {
        public TcpClient TcpClient { get; }

        public ServerClient(TcpClient client, string login)
        {
            TcpClient = client ?? throw new ArgumentNullException(nameof(client));
            Login = login ?? throw new ArgumentNullException(nameof(login));
        }

        public ServerClient()
        {
            
        }
    }
}