using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;

namespace P2P_projekt.Config
{
    /// <summary>
    /// Model class for storing application configuration settings.
    /// </summary>
    public class ConfigData
    {
        /// <summary>
        /// The node's IP address. Use "auto" to enable automatic detection.
        /// </summary>
        public string IpAddress { get; set; } = "auto";

        /// <summary>
        /// The communication port. Valid range is 65525 to 65535.
        /// </summary>
        public int Port { get; set; } = 65530;

        /// <summary>
        /// Network operation timeout in milliseconds.
        /// </summary>
        public int Timeout { get; set; } = 5000;

        /// <summary>
        /// The UI language code (e.g., "CZ", "EN").
        /// </summary>
        public string Language { get; set; } = "EN";

        /// <summary>
        /// A list of target IP addresses for network scanning operations.
        /// </summary>
        public List<string> TargetIps { get; set; } = new List<string>();
    }

    /// <summary>
    /// Static manager providing access to application settings and handling configuration I/O.
    /// </summary>
    public static class AppConfig
    {
        private static readonly string ConfigFolder = "Config";
        private static readonly string ConfigFileName = "config.json";
        private static readonly string ConfigPath = Path.Combine(ConfigFolder, ConfigFileName);

        private const int MinPort = 65525;
        private const int MaxPort = 65535;
        private const int DefaultPort = 65530;

        /// <summary>
        /// Gets the current application settings instance.
        /// </summary>
        public static ConfigData Settings { get; private set; } = new();

        /// <summary>
        /// Initializes the configuration by loading from disk or generating default settings.
        /// </summary>
        public static void Initialize()
        {
            if (!Directory.Exists(ConfigFolder)) Directory.CreateDirectory(ConfigFolder);

            if (File.Exists(ConfigPath))
            {
                try
                {
                    string json = File.ReadAllText(ConfigPath);
                    Settings = JsonSerializer.Deserialize<ConfigData>(json) ?? new();
                    ValidateSettings();
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
        /// Validates settings against allowed ranges and persists corrections if necessary.
        /// </summary>
        private static void ValidateSettings()
        {
            bool modified = false;

            if (Settings.Port < MinPort || Settings.Port > MaxPort)
            {
                Settings.Port = DefaultPort;
                modified = true;
            }

            if (Settings.Timeout < 100)
            {
                Settings.Timeout = 5000;
                modified = true;
            }

            string lang = Settings.Language.ToUpper();
            if (lang != "CZ" && lang != "EN")
            {
                Settings.Language = "EN";
                modified = true;
            }

            if (modified)
            {
                SaveCurrentConfig();
            }
        }

        /// <summary>
        /// Reverts settings to default values and saves them to the configuration file.
        /// </summary>
        private static void SaveDefaultConfig()
        {
            Settings = new ConfigData();
            SaveCurrentConfig();
        }

        /// <summary>
        /// Serializes the current settings to JSON and writes them to the disk.
        /// </summary>
        private static void SaveCurrentConfig()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(Settings, options);
            File.WriteAllText(ConfigPath, json);
        }

        /// <summary>
        /// Attempts to detect the local machine's IP address using a dummy UDP connection.
        /// </summary>
        /// <returns>The detected IP address as a string, or "127.0.0.1" if detection fails.</returns>
        private static string GetLocalIp()
        {
            try
            {
                using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
                socket.Connect("8.8.8.8", DefaultPort);
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