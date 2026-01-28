using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using P2P_projekt.Config;
using P2P_projekt.Core;
using P2P_projekt.Network;

namespace P2P_projekt.Commands
{
    /// <summary>
    /// Represents a command that scans the network for potential targets and calculates an optimal "robbery" plan 
    /// based on bank funds and client efficiency using asynchronous parallel processing.
    /// </summary>
    public class RobberyCommand : ICommand
    {
        private readonly long _targetAmount;
        private const string PrefixFunds = "BA";
        private const string PrefixClients = "BN";

        /// <summary>
        /// Data structure representing a bank node in the network.
        /// </summary>
        public record BankNode(string Ip, long Funds, int Clients)
        {
            /// <summary>
            /// Gets the efficiency ratio, calculated as funds per client.
            /// </summary>
            public double Efficiency => (double)Funds / (Clients == 0 ? 1 : Clients);
        }

        public RobberyCommand(string[] parts)
        {
            if (parts.Length < 2) throw new ArgumentException(Localization.Get("ErrFormat"));
            if (!long.TryParse(parts[1], out _targetAmount)) throw new ArgumentException(Localization.Get("ErrFormat"));
        }

        /// <summary>
        /// Executes the robbery command synchronously to maintain compatibility with the existing ICommand interface.
        /// </summary>
        public string Execute()
        {
            return ExecuteAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Asynchronously scans the network range using controlled parallelism to identify optimal victims.
        /// </summary>
        private async Task<string> ExecuteAsync()
        {
            var potentialTargets = GenerateIpRange();
            var victims = new ConcurrentBag<BankNode>();

            var options = new ParallelOptions { MaxDegreeOfParallelism = 20 };

            await Parallel.ForEachAsync(potentialTargets, options, async (ip, token) =>
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                try
                {
                    var node = await ScanNodeAsync(ip);
                    if (node != null && node.Funds > 0)
                    {
                        victims.Add(node);
                    }
                }
                catch
                {
                }
            });

            var sortedVictims = victims
                .OrderByDescending(v => v.Efficiency)
                .ThenByDescending(v => v.Funds)
                .ToList();

            if (!sortedVictims.Any())
            {
                return "RP Network poor, robbery impossible.";
            }

            return CalculateOptimalRobbery(sortedVictims);
        }

        /// <summary>
        /// Scans a specific IP address by requesting both funds (BA) and client counts (BN) in parallel.
        /// </summary>
        private async Task<BankNode?> ScanNodeAsync(string ip)
        {
            try
            {
                var taskBa = Task.Run(() => NetworkClient.SendRequest(ip, AppConfig.Settings.Port, "BA"));
                var taskBn = Task.Run(() => NetworkClient.SendRequest(ip, AppConfig.Settings.Port, "BN"));

                await Task.WhenAll(taskBa, taskBn);

                if (taskBa.Result.StartsWith("ER") || taskBn.Result.StartsWith("ER")) return null;

                if (TryParseResponse(taskBa.Result, PrefixFunds, out long funds) &&
                    TryParseResponse(taskBn.Result, PrefixClients, out long clients))
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
        /// Selects the best targets from the identified victims until the target amount is reached.
        /// </summary>
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

            string ipList = string.Join(" and ", targetIps);

            return string.Format(Localization.Get("MsgRobbery"), ipList, totalAffectedClients)
                   + $" (Total found: {currentSum} USD)";
        }

        private bool TryParseResponse(string? response, string prefix, out long value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(response) || !response.StartsWith(prefix)) return false;

            var parts = response.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 1 && long.TryParse(parts[1], out value);
        }

        /// <summary>
        /// Generates a list of target IP addresses, excluding the local machine's IP to prevent self-scanning.
        /// </summary>
        private List<string> GenerateIpRange()
        {
            var myIp = AppConfig.Settings.IpAddress;
            var ips = new HashSet<string>();

            if (AppConfig.Settings.TargetIps != null)
            {
                foreach (var ip in AppConfig.Settings.TargetIps)
                {
                    if (!string.IsNullOrWhiteSpace(ip) && ip.Trim() != myIp && ip.Trim() != "127.0.0.1")
                    {
                        ips.Add(ip.Trim());
                    }
                }
            }

            if (ips.Count == 0)
            {
                int lastDot = myIp.LastIndexOf('.');
                if (lastDot > 0)
                {
                    string prefix = myIp.Substring(0, lastDot + 1);
                    Logger.Instance.Log($"No custom targets. Auto-scanning subnet: {prefix}1 - {prefix}254");

                    for (int i = 1; i < 255; i++)
                    {
                        string target = prefix + i;
                        if (target != myIp)
                        {
                            ips.Add(target);
                        }
                    }
                }
            }

            return ips.ToList();
        }
    }
}