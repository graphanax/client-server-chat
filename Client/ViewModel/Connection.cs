using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Client.Annotations;
using Encryption;
using Prism.Commands;
using Server.ClientModel;

namespace Client.ViewModel
{
    public class Connection : INotifyPropertyChanged
    {
        public string ServerHost { get; set; }
        public int ServerPort { get; set; }
        public string Login { get; set; }

        private readonly string _clientHost;
        private readonly int _clientPort;

        private const int DefaultServerPort = 4095;

        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                OnPropertyChanged();
            }
        }

        public ICommand SendMessageCommand { get; set; }
        public ICommand ConnectCommand { get; set; }
        public ICommand DisconnectCommand { get; set; }
        public ObservableCollection<MessageItem> Messages { get; set; }

        private TcpClient _tcpClient;
        private NetworkStream _stream;
        private ConnectedClient _client;

        public bool CanConnect { get; set; }
        public bool CanDisconnect { get; set; }

        #region Encryption

        private readonly Rsa _rsa = new Rsa();

        public long LocalE => _rsa.e;
        public long LocalR => _rsa.r;

        public int RemoteE
        {
            get => _remoteE;
            set
            {
                _remoteE = value;
                OnPropertyChanged();
            }
        }

        public int RemoteR
        {
            get => _remoteR;
            set
            {
                _remoteR = value;
                OnPropertyChanged();
            }
        }

        private int _remoteE;
        private int _remoteR;
        private string _text;

        #endregion

        public Connection(IPAddress ip = null, int port = default)
        {
            _clientHost = ip?.ToString();
            _clientPort = port;

            ServerHost = GetLocalIpAddress().ToString();
            ServerPort = DefaultServerPort;

            CanConnect = true;
            CanDisconnect = false;

            Messages = new ObservableCollection<MessageItem>();

            SendMessageCommand = new DelegateCommand(SendMessage);
            ConnectCommand = new DelegateCommand(Connect);
            DisconnectCommand = new DelegateCommand(Disconnect);
        }

        private void Connect()
        {
            _tcpClient?.Close();

            try
            {
                if (_clientHost == null && _clientPort == 0)
                    _tcpClient = new TcpClient();
                else
                    _tcpClient = new TcpClient(_clientHost ?? string.Empty, _clientPort);
            }
            catch
            {
                MessageBox.Show("When creating a local socket, a fatal error occurred, restart the application.",
                    "Error!", MessageBoxButton.OK, MessageBoxImage.Error);

                return;
            }

            try
            {
                _tcpClient.Connect(ServerHost, ServerPort);
                _client = new ConnectedClient(_clientHost ?? GetLocalIpAddress().ToString(), Login);
                _stream = _tcpClient.GetStream();
            }
            catch
            {
                MessageBox.Show("An error occurred while trying to connect to the server.", "Error!",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                return;
            }

            Task.Factory.StartNew(() =>
            {
                try
                {
                    var bf = new BinaryFormatter();
                    bf.Serialize(_stream, _client);

                    while (_tcpClient.Connected)
                    {
                        var serverObject = bf.Deserialize(_stream);

                        switch (serverObject)
                        {
                            case ConnectedClient connectedClient:
                                if (connectedClient.IsOnline)
                                {
                                    CanConnect = false;
                                    CanDisconnect = true;

                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        Messages.Add(new MessageItem
                                        {
                                            Content = $"{connectedClient.Login} joined the chat.",
                                            SendTime = DateTime.Now
                                        });
                                    });

                                    OnPropertyChanged();
                                } else 
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        Messages.Add(new MessageItem
                                        {
                                            Content = $"{connectedClient.Login} left the chat.",
                                            SendTime = DateTime.Now
                                        });
                                    });

                                break;
                            case EncryptedMessage encryptedMessage:
                                if (encryptedMessage.Client.Login == _client.Login) continue;

                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    var text = _rsa.Decrypt(encryptedMessage.Message.Content);
                                    Messages.Add(encryptedMessage.EncMessageToMessageItem(text));
                                }, DispatcherPriority.Background);
                                break;
                        }
                    }
                }
                catch
                {
                    ShowDisconnectMessage();
                    Disconnect();
                }
            }, TaskCreationOptions.LongRunning);
        }

        private void SendMessage()
        {
            try
            {
                var data = _rsa.Encrypt(Text, RemoteE, RemoteR);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Messages.Add(new MessageItem {Login = _client.Login, Content = Text, SendTime = DateTime.Now});
                });

                var message = new EncryptedMessage(new Message(data, DateTime.Now), _client);
                new BinaryFormatter().Serialize(_stream, message);

                Text = string.Empty;
                OnPropertyChanged(nameof(Text));
            }
            catch
            {
                MessageBox.Show("The message was not sent.", "Error!",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Disconnect()
        {
            _tcpClient?.Close();
            CanConnect = true;
            CanDisconnect = false;
            OnPropertyChanged();
        }

        private void ShowDisconnectMessage()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Messages.Add(
                    new MessageItem {Content = "The connection is interrupted...", SendTime = DateTime.Now});
            });
        }

        private static IPAddress GetLocalIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip;

            throw new Exception("No network adapters with an IPv4 address in the system");
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}