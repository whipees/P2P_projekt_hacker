using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using P2P_projekt.Config;
using System.Windows;
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

        public async Task<string> ExecuteAsync()
        {
            var potentialTargets = GenerateIpRange();

            
            var scanTasks = potentialTargets.Select(ScanNodeAsync);
            var results = await Task.WhenAll(scanTasks);

           
            var victims = results
                .Where(v => v != null)
                .OrderByDescending(v => v.Funds)
                .ToList();

            if (!victims.Any())
                return "Network poor, robbery impossible.";

            return CalculateOptimalRobbery(victims);
        }

        private async Task<BankNode?> ScanNodeAsync(string ip)
        {
            try
            {
                
                var taskBa = Task.Run(() => NetworkClient.SendRequest(ip, AppConfig.Settings.Port, PrefixFunds));
                var taskBn = Task.Run(() => NetworkClient.SendRequest(ip, AppConfig.Settings.Port, PrefixClients));

                await Task.WhenAll(taskBa, taskBn);

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
                   + $" (Total: {currentSum} USD)";
        }

        private bool TryParseResponse(string response, string prefix, out long value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(response) || !response.StartsWith(prefix)) return false;

            var parts = response.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 1 && long.TryParse(parts[1], out value);
        }

        private IEnumerable<string> GenerateIpRange() => new[] { "127.0.0.1", AppConfig.Settings.IpAddress };

       
        public string Execute() => ExecuteAsync().GetAwaiter().GetResult();
    }
}