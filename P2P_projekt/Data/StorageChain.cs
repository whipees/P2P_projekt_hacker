using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using P2P_projekt.Core;

namespace P2P_projekt.Data
{
    /// <summary>
    /// Provides a robust, redundant storage mechanism for bank account data.
    /// Implements atomic writes and backup rotation to ensure data integrity.
    /// </summary>
    public class StorageChain : IStorage
    {
        private const string PrimaryFile = "bank_data.json";
        private const string BackupFile = "bank_data.bak";

        /// <summary>
        /// Loads account data from storage. Priority is given to the primary file; 
        /// if corrupted or missing, it falls back to the latest backup.
        /// </summary>
        /// <returns>A dictionary containing account IDs and balances.</returns>
        public Dictionary<int, long> Load()
        {
            try
            {
                if (File.Exists(PrimaryFile))
                {
                    try
                    {
                        return LoadFromFile(PrimaryFile);
                    }
                    catch (JsonException)
                    {
                        Logger.Instance.Error("Primary storage file is corrupted. Attempting to load from backup.");
                    }
                }

                if (File.Exists(BackupFile))
                {
                    return LoadFromFile(BackupFile);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Critical failure during data load: {ex.Message}");
            }

            return new Dictionary<int, long>();
        }

        /// <summary>
        /// Reads and deserializes JSON data from the specified file path.
        /// </summary>
        /// <param name="path">The file path to read.</param>
        /// <returns>The deserialized account dictionary.</returns>
        private Dictionary<int, long> LoadFromFile(string path)
        {
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Dictionary<int, long>>(json) ?? new Dictionary<int, long>();
        }

        /// <summary>
        /// Saves account data using an atomic write pattern. 
        /// Existing primary data is rotated to a backup before the new state is committed.
        /// </summary>
        /// <param name="accounts">The dictionary of accounts to persist.</param>
        public void Save(Dictionary<int, long> accounts)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(accounts, options);

                if (File.Exists(PrimaryFile))
                {
                    File.Copy(PrimaryFile, BackupFile, true);
                }

                string tempFile = PrimaryFile + ".tmp";
                File.WriteAllText(tempFile, json);

                if (File.Exists(PrimaryFile))
                {
                    File.Delete(PrimaryFile);
                }

                File.Move(tempFile, PrimaryFile);

                Logger.Instance.Log("Data successfully persisted and backup rotated.");
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"CRITICAL STORAGE ERROR: {ex.Message}");
            }
        }
    }
}