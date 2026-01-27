using P2P_projekt.Core;
using P2PBankNode.Core;
using System.IO;
using System.Text.Json;

namespace P2PBankNode.Data
{
    public class StorageChain : IStorage
    {
        private const string JsonFile = "bank_data.json";
        private const string CsvFile = "bank_data.csv";

        public Dictionary<int, long> Load()
        {
            try
            {
                if (File.Exists(JsonFile))
                {
                    string json = File.ReadAllText(JsonFile);
                    return JsonSerializer.Deserialize<Dictionary<int, long>>(json) ?? new Dictionary<int, long>();
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"JSON Load failed: {ex.Message}");
            }
            return new Dictionary<int, long>();
        }

        public void Save(Dictionary<int, long> accounts)
        {
            bool saved = false;

            try
            {
                string json = JsonSerializer.Serialize(accounts);
                File.WriteAllText(JsonFile, json);
                saved = true;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Primary Storage Failed: {ex.Message}");
            }

            if (!saved)
            {
                try
                {
                    using StreamWriter sw = new StreamWriter(CsvFile);
                    foreach (var kvp in accounts)
                    {
                        sw.WriteLine($"{kvp.Key},{kvp.Value}");
                    }
                    saved = true;
                }
                catch (Exception ex)
                {
                    Logger.Instance.Error($"Secondary Storage Failed: {ex.Message}");
                }
            }

            if (!saved)
            {
                Logger.Instance.Error("CRITICAL: Running in Memory-Only mode.");
            }
        }
    }
}