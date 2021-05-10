using System.Net;
using System.Windows;
using Client.View;
using Client.ViewModel;

namespace Client
{
    public partial class App
    {
        private void OnStartup(object sender, StartupEventArgs e)
        {
            Connection connection;

            if (e.Args.Length == 2 &&
                IPAddress.TryParse(e.Args[0], out var host) &&
                int.TryParse(e.Args[1], out var port))
            {                
                connection = new Connection(host, port);
            }
            else
            {
                connection = new Connection();
            }
            new MainWindow { DataContext = connection }.ShowDialog();
        }
    }
}