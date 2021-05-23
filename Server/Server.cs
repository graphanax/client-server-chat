using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using Server.ClientModel;

namespace Server
{
    public delegate void OutputMessageHandler(string text);

    public class Server
    {
        private readonly TcpListener _tcpListener;
        private static List<ServerClient> _clients;
        private static OutputMessageHandler _delOutput;

        public Server(IPAddress address, int port, OutputMessageHandler delegateOutput)
        {
            _tcpListener = new TcpListener(address, port);
            _clients = new List<ServerClient>();
            _delOutput = delegateOutput;

            _delOutput($"Starting the server on {address}:{port}...\n");
        }

        public void Start()
        {
            try
            {
                _tcpListener.Start();
                _delOutput("Server is running successfully!\n");
            }
            catch (Exception e)
            {
                _delOutput($"Failed to start server: {e.Message}\n");
                return;
            }

            try
            {
                while (true)
                {
                    var client = _tcpListener.AcceptTcpClient();

                    Task.Factory.StartNew(() =>
                    {
                        using var stream = client.GetStream();
                        var bf = new BinaryFormatter();

                        var serverClient = new ServerClient();

                        var connectedClient = bf.Deserialize(stream) as ConnectedClient;

                        lock (_clients)
                        {
                            if (_clients.Exists(c => connectedClient != null &&
                                                     c.Login == connectedClient.Login))
                            {
                                _delOutput("The connection to the client connected with a username " +
                                           "that is already occupied was not established.\n");
                                
                                client.Client.Disconnect(true);
                                Thread.CurrentThread.Abort();
                            }
                            else
                            {
                                if (connectedClient == null) return;

                                serverClient = new ServerClient(client, connectedClient.Login);
                                _clients.Add(serverClient);
                                
                                WriteAboutNewConnection(connectedClient);
                                SendToAllClients(connectedClient);
                            }
                        }
                        
                        while (client.Client.Connected)
                        {
                            try
                            {
                                var encMess = bf.Deserialize(stream) as EncryptedMessage;

                                WriteText(encMess);
                                SendToAllClients(encMess);
                            }
                            catch
                            {
                                if (client.Client.Connected)
                                {
                                    client.Client.Disconnect(true);
                                }
                            }
                        }

                        lock (_clients)
                        {
                            _clients.Remove(serverClient);
                        }

                        _delOutput($"Disconnection: {serverClient.Login}.\n");
                        WriteAboutDisconnection(serverClient);
                    }, TaskCreationOptions.LongRunning);
                }
            }
            catch (Exception e)
            {
                _delOutput($"An unexpected error occurred: {e.Message}\n");
            }
        }

        private static async void SendToAllClients(object serverObj)
        {
            await Task.Factory.StartNew(() =>
            {
                lock (_clients)
                {
                    var bf = new BinaryFormatter();
                    foreach (var client in _clients)
                    {
                        try
                        {
                            if (client.TcpClient.Connected)
                            {
                                bf.Serialize(client.TcpClient.GetStream(), serverObj);
                            }
                            else
                            {
                                _clients.Remove(client);
                            }
                        }
                        catch (Exception e)
                        {
                            client.TcpClient.Client.Disconnect(false);
                            _clients.Remove(client);
                            _delOutput(e.Message);
                        }
                    }
                }
            });
        }
        
        private static void WriteAboutNewConnection(ConnectedClient client)
        {
            _delOutput($"New connection: {client.Login} ({client.Host}).\n");
        }

        private static void WriteAboutDisconnection(ServerClient client)
        {
            var conClient = new ConnectedClient(string.Empty, client.Login) {IsOnline = false};
            SendToAllClients(conClient);
        }

        private static void WriteText(EncryptedMessage message)
        {
            _delOutput($"{message.Client.Login} " +
                       $"[{message.Message.SendTime.ToString(CultureInfo.InvariantCulture)}]: " +
                       $"{string.Join(" ", message.Message.Content)}\n");
        }
    }
}