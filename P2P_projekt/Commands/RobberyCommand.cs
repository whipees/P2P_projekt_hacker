using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using P2P_projekt.Config;
using P2P_projekt.Core;
using P2P_projekt.Network;

namespace P2P_projekt.Commands
{
    /// <summary>
    /// Represents a command that scans the network for potential targets and calculates an optimal "robbery" plan 
    /// based on bank funds and client efficiency.
    /// </summary>
    public class RobberyCommand : ICommand
    {
        private readonly long _targetAmount;
        private const string PrefixFunds = "BA";
        private const string PrefixClients = "BN";

        /// <summary>
        /// Data structure representing a bank node in the network.
        /// </summary>
        /// <param name="Ip">The IP address of the bank.</param>
        /// <param name="Funds">Total funds available at the bank.</param>
        /// <param name="Clients">Total number of clients at the bank.</param>
        public record BankNode(string Ip, long Funds, int Clients)
        {
            /// <summary>
            /// Gets the efficiency ratio, calculated as funds per client.
            /// </summary>
            public double Efficiency => (double)Funds / (Clients == 0 ? 1 : Clients);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RobberyCommand"/> class.
        /// </summary>
        /// <param name="parts">The split command arguments, where the second element is the target amount.</param>
        /// <exception cref="ArgumentException">Thrown when the input format is invalid.</exception>
        public RobberyCommand(string[] parts)
        {
            if (parts.Length < 2) throw new ArgumentException(Localization.Get("ErrFormat"));
            if (!long.TryParse(parts[1], out _targetAmount)) throw new ArgumentException(Localization.Get("ErrFormat"));
        }

        /// <summary>
        /// Executes the robbery command synchronously by blocking the async task.
        /// </summary>
        /// <returns>A string summary of the robbery plan or a failure message.</returns>
        public string Execute()
        {
            return ExecuteAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Asynchronously scans the network range and identifies optimal victims.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, containing the robbery result string.</returns>
        private async Task<string> ExecuteAsync()
        {
            var potentialTargets = GenerateIpRange();

            var scanTasks = potentialTargets.Select(ScanNodeAsync);
            var results = await Task.WhenAll(scanTasks);

            var victims = results
                .Where(v => v != null && v.Funds > 0)
                .OrderByDescending(v => v.Efficiency)
                .ThenByDescending(v => v.Funds)
                .ToList();

            if (!victims.Any())
            {
                Logger.Instance.Log($"Robbery scan finished. Scanned {potentialTargets.Count} IPs, found 0 valid victims.");
                return "RP Network poor, robbery impossible.";
            }

            return CalculateOptimalRobbery(victims);
        }

        /// <summary>
        /// Scans an individual IP address for bank information.
        /// </summary>
        /// <param name="ip">The IP address to scan.</param>
        /// <returns>A <see cref="BankNode"/> if the scan is successful; otherwise, null.</returns>
        private async Task<BankNode?> ScanNodeAsync(string ip)
        {
            try
            {
                var t1 = Task.Run(() => NetworkClient.SendRequest(ip, AppConfig.Settings.Port, "BA"));
                var t2 = Task.Run(() => NetworkClient.SendRequest(ip, AppConfig.Settings.Port, "BN"));

                await Task.WhenAll(t1, t2);

                string rawBa = t1.Result;
                string rawBn = t2.Result;

                if (rawBa.StartsWith("ER") || rawBn.StartsWith("ER")) return null;

                if (TryParseResponse(rawBa, PrefixFunds, out long funds) &&
                    TryParseResponse(rawBn, PrefixClients, out long clients))
                {
                    return new BankNode(ip, funds, (int)clients);
                }
            }
            catch
            {
            }
            return null;
        }

        /// <summary>
        /// Calculates which banks should be targeted to reach the target amount with maximum efficiency.
        /// </summary>
        /// <param name="victims">A sorted list of potential target banks.</param>
        /// <returns>A formatted string detailing the target IPs and total affected clients.</returns>
        private string CalculateOptimalRobbery(List<BankNode> victims)
        {
            long currentSum = 0;
            int totalAffectedClients = 0;
            var targetIps = new List<string>();

            foreach (var victim in victims)
            {
                targetIps.Add(victim.Ip);
                currentSum += victim.Funds;
                totalAffectedClients += victim.Clients;

                if (currentSum >= _targetAmount) break;
            }

            string ipList = string.Join(" a ", targetIps);

            return string.Format(Localization.Get("MsgRobbery"), ipList, totalAffectedClients)
                   + $" (Total found: {currentSum} USD)";
        }

        /// <summary>
        /// Parses a raw network response to extract a long value based on a specific prefix.
        /// </summary>
        /// <param name="response">The raw string response from the network.</param>
        /// <param name="prefix">The expected protocol prefix (e.g., BA or BN).</param>
        /// <param name="value">The extracted value if parsing is successful.</param>
        /// <returns>True if parsing was successful; otherwise, false.</returns>
        private bool TryParseResponse(string? response, string prefix, out long value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(response) || !response.StartsWith(prefix)) return false;

            var parts = response.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 1 && long.TryParse(parts[1], out value);
        }

        /// <summary>
        /// Generates a list of IP addresses to scan, either from configuration or by auto-scanning the local subnet.
        /// </summary>
        /// <returns>A list of IP addresses as strings.</returns>
        private List<string> GenerateIpRange()
        {
            var ips = new List<string> { "127.0.0.1" };

            if (!ips.Contains(AppConfig.Settings.IpAddress))
                ips.Add(AppConfig.Settings.IpAddress);

            bool customTargetsFound = false;

            if (AppConfig.Settings.TargetIps != null && AppConfig.Settings.TargetIps.Count > 0)
            {
                foreach (var ip in AppConfig.Settings.TargetIps)
                {
                    if (!string.IsNullOrWhiteSpace(ip) && !ips.Contains(ip))
                    {
                        ips.Add(ip.Trim());
                        customTargetsFound = true;
                    }
                }
            }

            if (!customTargetsFound)
            {
                string localIp = AppConfig.Settings.IpAddress;
                int lastDot = localIp.LastIndexOf('.');

                if (lastDot > 0)
                {
                    string prefix = localIp.Substring(0, lastDot + 1);
                    Logger.Instance.Log($"Config empty. Auto-scanning subnet: {prefix}1 - {prefix}254");

                    for (int i = 1; i < 255; i++)
                    {
                        string target = prefix + i;
                        if (!ips.Contains(target))
                        {
                            ips.Add(target);
                        }
                    }
                }
            }

            return ips;
        }
    }
}