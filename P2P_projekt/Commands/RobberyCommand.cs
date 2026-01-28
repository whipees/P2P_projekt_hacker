using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using P2P_projekt.Config;
using P2P_projekt.Core;
using P2P_projekt.Network;

namespace P2P_projekt.Commands
{
    public class RobberyCommand : ICommand
    {
        private readonly long _targetAmount;
        private const string PrefixFunds = "BA";
        private const string PrefixClients = "BN";

        public record BankNode(string Ip, long Funds, int Clients);

        public RobberyCommand(string[] parts)
        {
            if (parts.Length < 2) throw new ArgumentException(Localization.Get("ErrFormat"));
            if (!long.TryParse(parts[1], out _targetAmount)) throw new ArgumentException(Localization.Get("ErrFormat"));
        }

        public string Execute()
        {
            return ExecuteAsync().GetAwaiter().GetResult();
        }

        private async Task<string> ExecuteAsync()
        {
            var potentialTargets = GenerateIpRange();

            // Spustíme skenování všech IP
            var scanTasks = potentialTargets.Select(ScanNodeAsync);
            var results = await Task.WhenAll(scanTasks);

            // Vybereme jen ty, co se podařilo spojit
            var victims = results
                .Where(v => v != null)
                .OrderByDescending(v => v.Funds)
                .ToList();

            if (!victims.Any())
            {
                // Pokud skenování selhalo, vypíšeme proč do logu (pro debugování)
                Logger.Instance.Error("Robbery failed: No accessible banks found via Network Scan.");
                return "Network poor, robbery impossible. (Check node.log for details)";
            }

            return CalculateOptimalRobbery(victims);
        }

        private async Task<BankNode?> ScanNodeAsync(string ip)
        {
            try
            {
                // OPRAVA: Použití AppConfig.Settings.Port
                var t1 = Task.Run(() => NetworkClient.SendRequest(ip, AppConfig.Settings.Port, "BA"));
                var t2 = Task.Run(() => NetworkClient.SendRequest(ip, AppConfig.Settings.Port, "BN"));

                await Task.WhenAll(t1, t2);

                string rawBa = t1.Result;
                string rawBn = t2.Result;

                // Debug logování, pokud dostaneme chybu
                if (rawBa.StartsWith("ER") || rawBn.StartsWith("ER"))
                {
                    Logger.Instance.Error($"Scan error for {ip}: BA='{rawBa}', BN='{rawBn}'");
                    return null;
                }

                if (TryParseResponse(rawBa, PrefixFunds, out long funds) &&
                    TryParseResponse(rawBn, PrefixClients, out long clients))
                {
                    return new BankNode(ip, funds, (int)clients);
                }
            }
            catch (Exception ex)
            {
                // Zaznamenáme chybu spojení
                Logger.Instance.Error($"Scan exception for {ip}: {ex.Message}");
            }
            return null;
        }

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

            string ipList = string.Join(", ", targetIps);

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

        private List<string> GenerateIpRange()
        {
            return new List<string>
            {
                "127.0.0.1",
                AppConfig.Settings.IpAddress
            };
        }
    }
}