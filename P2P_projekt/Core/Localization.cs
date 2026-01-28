using System.Collections.Generic;

namespace P2P_projekt.Core
{
    public static class Localization
    {
        public static string CurrentLanguage { get; set; } = "EN";

        private static readonly Dictionary<string, Dictionary<string, string>> _dictionary = new()
        {
            ["EN"] = new()
            {
                ["StatusOnline"] = "ONLINE - RUNNING",
                ["StatusOffline"] = "OFFLINE - STOPPED",
                ["Funds"] = "Total Funds (USD)",
                ["Clients"] = "Active Clients",
                ["Ip"] = "IP Address",
                ["Shutdown"] = "SAFE SHUTDOWN",
                ["ErrFormat"] = "Invalid command format.",
                ["ErrAccount"] = "Account does not exist.",
                ["ErrFunds"] = "Insufficient funds (USD).",
                ["ErrExists"] = "Account already exists.",
                ["ErrNotEmpty"] = "Account is not empty.",
                ["ErrInternal"] = "Internal server error.",
                ["MsgRobbery"] = "To reach target, rob banks: {0}. Victims: {1}."
            },
            ["CZ"] = new()
            {
                ["StatusOnline"] = "ONLINE - BĚŽÍ",
                ["StatusOffline"] = "OFFLINE - ZASTAVENO",
                ["Funds"] = "Celkem peněz (USD)",
                ["Clients"] = "Počet klientů",
                ["Ip"] = "IP Adresa",
                ["Shutdown"] = "BEZPEČNÉ VYPNUTÍ",
                ["ErrFormat"] = "Neplatný formát příkazu.",
                ["ErrAccount"] = "Účet neexistuje.",
                ["ErrFunds"] = "Nedostatek prostředků (USD).",
                ["ErrExists"] = "Účet již existuje.",
                ["ErrNotEmpty"] = "Účet není prázdný.",
                ["ErrInternal"] = "Interní chyba serveru.",
                ["MsgRobbery"] = "K dosažení cíle vyloupit: {0}. Obětí: {1}."
            }
        };

        public static string Get(string key)
        {
            if (_dictionary.ContainsKey(CurrentLanguage) && _dictionary[CurrentLanguage].ContainsKey(key))
            {
                return _dictionary[CurrentLanguage][key];
            }
            return key;
        }

        public static void ToggleLanguage()
        {
            CurrentLanguage = (CurrentLanguage == "EN") ? "CZ" : "EN";
        }
    }
}