using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using P2P_projekt.Core;

namespace P2P_projekt.Data
{
    public class StorageChain : IStorage
    {
        private const string PrimaryFile = "bank_data.json";
        private const string BackupFile = "bank_data.bak";

        public Dictionary<int, long> Load()
        {
            try
            {
                if (File.Exists(PrimaryFile))
                    return LoadFromFile(PrimaryFile);

                if (File.Exists(BackupFile))
                {
                    Logger.Instance.Log("Primary storage missing. Loading from Backup.");
                    return LoadFromFile(BackupFile);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Load failed: {ex.Message}");
            }
            return new Dictionary<int, long>();
        }

        private Dictionary<int, long> LoadFromFile(string path)
        {
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Dictionary<int, long>>(json) ?? new Dictionary<int, long>();
        }

        public void Save(Dictionary<int, long> accounts)
        {
            bool saved = false;

            try
            {
                string json = JsonSerializer.Serialize(accounts);
                File.WriteAllText(PrimaryFile, json);
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
                    string json = JsonSerializer.Serialize(accounts);
                    File.WriteAllText(BackupFile, json);
                    saved = true;
                    Logger.Instance.Log("Saved to Backup Storage.");
                }
                catch (Exception ex)
                {
                    Logger.Instance.Error($"Backup Storage Failed: {ex.Message}");
                }
            }

            if (!saved)
            {
                Logger.Instance.Error("CRITICAL: Storage failed. Data valid in RAM only!");
            }
        }
    }
}