using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using P2P_projekt.Core;

namespace P2P_projekt.Data
{
    public class StorageChain : IStorage
    {
        private const string JsonFile = "bank_data.json";
        private const string BackupFile = "bank_data.bak";

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
                Logger.Instance.Error($"Persistence load failed: {ex.Message}");
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
                Logger.Instance.Error($"Primary storage failed: {ex.Message}");
            }

            if (!saved)
            {
                try
                {
                    string json = JsonSerializer.Serialize(accounts);
                    File.WriteAllText(BackupFile, json);
                    saved = true;
                }
                catch (Exception ex)
                {
                    Logger.Instance.Error($"Backup storage failed: {ex.Message}");
                }
            }

            if (!saved)
            {
                Logger.Instance.Error("CRITICAL: Data held in memory only.");
            }
        }
    }
}