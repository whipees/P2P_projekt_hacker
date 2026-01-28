using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using P2P_projekt.Core;

namespace P2P_projekt.Data
{
    /// <summary>
    /// Implements a redundant storage mechanism that uses a primary JSON file and a backup file
    /// to ensure data persistence for bank accounts.
    /// </summary>
    public class StorageChain : IStorage
    {
        private const string PrimaryFile = "bank_data.json";
        private const string BackupFile = "bank_data.bak";

        /// <summary>
        /// Loads account data from the primary storage file. If the primary file is missing,
        /// it attempts to load data from the backup file.
        /// </summary>
        /// <returns>A dictionary of account IDs and their balances; returns an empty dictionary if both files fail.</returns>
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

        /// <summary>
        /// Reads and deserializes JSON data from a specific file path.
        /// </summary>
        /// <param name="path">The file path to read from.</param>
        /// <returns>A deserialized dictionary of account data.</returns>
        private Dictionary<int, long> LoadFromFile(string path)
        {
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Dictionary<int, long>>(json) ?? new Dictionary<int, long>();
        }

        /// <summary>
        /// Persists account data to the primary storage file. If the primary storage fails,
        /// it attempts to save the data to the backup file.
        /// </summary>
        /// <param name="accounts">The dictionary of accounts and balances to be saved.</param>
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