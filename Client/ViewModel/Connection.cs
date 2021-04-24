using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Prism.Commands;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Client.Annotations;
using Encryption;
using Server.ClientModel;

namespace Client.ViewModel
{
    public class Connection : INotifyPropertyChanged
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Login { get; set; }

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

        #region Encryption

        private readonly Rsa _rsa = new Rsa();

        public int RemoteE
        {
            get => _remoteE;
            set
            {
                _remoteE = value;
                OnPropertyChanged();
            }
        }

        public int RemoteN
        {
            get => _remoteN;
            set
            {
                _remoteN = value;
                OnPropertyChanged();
            }
        }

        private int _remoteE;
        private int _remoteN;
        private string _text;

        #endregion

        public Connection(IPAddress ip, int port)
        {
            Host = ip.ToString();
            Port = port;

            Messages = new ObservableCollection<MessageItem>();

            SendMessageCommand = new DelegateCommand(SendMessage);
            ConnectCommand = new DelegateCommand(Connect);
            DisconnectCommand = new DelegateCommand(Disconnect);
        }

        private void Connect()
        {
            _tcpClient?.Close();
            _tcpClient = new TcpClient();

            try
            {
                _tcpClient.Connect(Host, Port);
            }
            catch
            {
                MessageBox.Show("An error occurred while trying to connect to the server.", "Error!",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                return;
            }

            _stream = _tcpClient.GetStream();
            _client = new ConnectedClient(GetLocalIpAddress().ToString(), Login);

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
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    Messages.Add(new MessageItem
                                    {
                                        Content = $"New connection: {connectedClient.Login}", SendTime = DateTime.Now
                                    });
                                });

                                OnPropertyChanged();
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
                }
            }, TaskCreationOptions.LongRunning);
        }

        private void SendMessage()
        {
            try
            {
                var data = _rsa.Encrypt(Text, RemoteE, RemoteN);

                Dispatcher.CurrentDispatcher.Invoke(() =>
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
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}