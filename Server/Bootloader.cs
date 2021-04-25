using System;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    internal static class Bootloader
    {
        private const int DefaultPort = 4095;
        
        public static void Main(string[] args)
        {
            if (args.Length == 2 &&
                IPAddress.TryParse(args[0], out var host) &&
                int.TryParse(args[1], out var port))
            {
                new Server(host, port, Display).Start();
            }
            else
            {
                new Server(GetLocalIpAddress(), DefaultPort, Display).Start();
            }
        }

        private static void Display(string text)
        {
            Console.Write(text);
        }

        private static IPAddress GetLocalIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip;

            throw new Exception("No network adapters with an IPv4 address in the system");
        }
    }
}