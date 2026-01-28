using P2P_projekt.Core;
using P2P_projekt.Config;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace P2P_projekt.Network
{
    /// <summary>
    /// Provides static methods for handling synchronous network requests over TCP.
    /// </summary>
    public static class NetworkClient
    {
        /// <summary>
        /// Sends a string command to a remote TCP server and waits for a single-line response.
        /// </summary>
        /// <param name="ip">The destination IP address.</param>
        /// <param name="port">The destination TCP port.</param>
        /// <param name="command">The raw command string to send.</param>
        /// <returns>
        /// The trimmed response string from the server if successful; 
        /// otherwise, an error message prefixed with "ER".
        /// </returns>
        public static string SendRequest(string ip, int port, string command)
        {
            try
            {
                using TcpClient client = new TcpClient();

                if (!client.ConnectAsync(ip, port).Wait(AppConfig.Settings.Timeout))
                {
                    return "ER Connection timed out";
                }

                using NetworkStream stream = client.GetStream();
                stream.ReadTimeout = AppConfig.Settings.Timeout;
                stream.WriteTimeout = AppConfig.Settings.Timeout;

                using StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
                writer.WriteLine(command);

                using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                string? response = reader.ReadLine();

                if (string.IsNullOrWhiteSpace(response))
                {
                    return "ER Empty response";
                }

                return response.Trim();
            }
            catch (Exception ex)
            {
                return $"ER Network error: {ex.Message}";
            }
        }
    }
}