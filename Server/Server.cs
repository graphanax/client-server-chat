using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
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
                        var stream = client.GetStream();
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
                            }
                            else
                            {
                                if (connectedClient == null) return;

                                serverClient = new ServerClient(client, connectedClient.Id, connectedClient.Login);
                                _clients.Add(serverClient);

                                WriteAboutNewConnection(connectedClient);
                                SendToAllClients(connectedClient);
                            }
                        }

                        while (client.Client.Connected)
                        {
                            var encMess = bf.Deserialize(stream) as EncryptedMessage;

                            WriteText(encMess);
                            SendToAllClients(encMess);
                        }

                        lock (_clients)
                        {
                            _clients.Remove(serverClient);
                        }
                    }, TaskCreationOptions.LongRunning);
                }
            }
            catch (Exception e)
            {
                _delOutput(e.Message);
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

        private void WriteAboutNewConnection(ConnectedClient client)
        {
            _delOutput($"New connection: {client.Login} - {client.Source}\n");
        }

        private static void WriteText(EncryptedMessage message)
        {
            _delOutput($"{message.Client.Login} " +
                       $"[{message.Message.SendTime.ToString()}]: " +
                       $"{string.Join(" ", message.Message.Content)}");
        }
    }
}