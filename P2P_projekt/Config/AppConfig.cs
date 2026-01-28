using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;

namespace P2P_projekt.Config
{
    /// <summary>
    /// Represents the data structure for application configuration settings.
    /// </summary>
    public class ConfigData
    {
        /// <summary>
        /// Gets or sets the IP address. Use "auto" to automatically detect the local IP.
        /// </summary>
        public string IpAddress { get; set; } = "auto";

        /// <summary>
        /// Gets or sets the communication port.
        /// </summary>
        public int Port { get; set; } = 65530;

        /// <summary>
        /// Gets or sets the network timeout in milliseconds.
        /// </summary>
        public int Timeout { get; set; } = 5000;

        /// <summary>
        /// Gets or sets the application language code (e.g., "EN", "CZ").
        /// </summary>
        public string Language { get; set; } = "EN";

        /// <summary>
        /// Gets or sets the list of target IP addresses for network scanning or robbery operations.
        /// </summary>
        public List<string> TargetIps { get; set; } = new List<string>();
    }

    /// <summary>
    /// Static manager providing access to application configuration and handling file I/O for settings.
    /// </summary>
    public static class AppConfig
    {
        private static readonly string ConfigFolder = "Config";
        private static readonly string ConfigFileName = "config.json";

        private static readonly string ConfigPath = Path.Combine(ConfigFolder, ConfigFileName);

        /// <summary>
        /// Gets the current application settings.
        /// </summary>
        public static ConfigData Settings { get; private set; } = new();

        /// <summary>
        /// Initializes the configuration by loading it from a file or creating a default one if it doesn't exist.
        /// </summary>
        public static void Initialize()
        {
            if (!Directory.Exists(ConfigFolder))
            {
                Directory.CreateDirectory(ConfigFolder);
            }

            if (File.Exists(ConfigPath))
            {
                try
                {
                    string json = File.ReadAllText(ConfigPath);
                    Settings = JsonSerializer.Deserialize<ConfigData>(json) ?? new();
                }
                catch
                {
                    SaveDefaultConfig();
                }
            }
            else
            {
                SaveDefaultConfig();
            }

            if (Settings.IpAddress.ToLower() == "auto")
            {
                Settings.IpAddress = GetLocalIp();
            }
        }

        /// <summary>
        /// Generates and saves a default configuration file.
        /// </summary>
        private static void SaveDefaultConfig()
        {
            Settings = new ConfigData();
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(Settings, options);
            File.WriteAllText(ConfigPath, json);
        }

        /// <summary>
        /// Attempts to determine the local machine's IP address by connecting to an external endpoint.
        /// </summary>
        /// <returns>The local IP address as a string, or "127.0.0.1" if detection fails.</returns>
        private static string GetLocalIp()
        {
            try
            {
                using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
                socket.Connect("8.8.8.8", 65530);
                if (socket.LocalEndPoint is IPEndPoint endPoint)
                {
                    return endPoint.Address.ToString();
                }
                return "127.0.0.1";
            }
            catch
            {
                return "127.0.0.1";
            }
        }
    }
}