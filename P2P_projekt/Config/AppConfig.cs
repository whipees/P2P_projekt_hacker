using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;

namespace P2P_projekt.Core
{
    public class ConfigData
    {
        public string IpAddress { get; set; } = "auto";
        public int Port { get; set; } = 65530;
        public int Timeout { get; set; } = 5000;
        public string Language { get; set; } = "EN";
    }

    public static class AppConfig
    {
        private static readonly string ConfigFolder = "Config";
        private static readonly string ConfigFileName = "config.json";

        private static readonly string ConfigPath = Path.Combine(ConfigFolder, ConfigFileName);

        public static ConfigData Settings { get; private set; } = new();

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

        private static void SaveDefaultConfig()
        {
            Settings = new ConfigData();
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(Settings, options);
            File.WriteAllText(ConfigPath, json);
        }

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