using P2P_projekt.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace P2P_projekt.Network
{
    public static class NetworkClient
    {
        public static string SendRequest(string ip, int port, string command)
        {
            try
            {
                using TcpClient client = new TcpClient();
                if (!client.ConnectAsync(ip, port).Wait(AppConfig.Timeout))
                {
                    return "ER Connection timed out";
                }

                using NetworkStream stream = client.GetStream();
                stream.ReadTimeout = AppConfig.Timeout;
                stream.WriteTimeout = AppConfig.Timeout;

                byte[] data = Encoding.UTF8.GetBytes(command + "\n");
                stream.Write(data, 0, data.Length);

                byte[] buffer = new byte[4096];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);

                return Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
            }
            catch (Exception ex)
            {
                return $"ER Network error: {ex.Message}";
            }
        }
    }
}
