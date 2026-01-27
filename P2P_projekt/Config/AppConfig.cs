using System.Net;
using System.Net.Sockets;

namespace P2P_projekt.Config
{
    public static class AppConfig
    {
        public static string IpAddress { get; private set; } = GetLocalIp();
        public static int Port { get; set; } = 65530;
        public static int Timeout { get; set; } = 5000;
        public static string Language { get; set; } = "EN";

        private static string GetLocalIp()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();
            }
            return "127.0.0.1";
        }
    }
}