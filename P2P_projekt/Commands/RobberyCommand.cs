using System;
using System.Collections.Generic;
using System.Linq;
using P2P_projekt.Core;
using P2P_projekt.Network;
using P2P_projekt.Config;

namespace P2P_projekt.Commands
{
    public class RobberyCommand : ICommand
    {
        private readonly long _targetAmount;

        public RobberyCommand(string[] parts)
        {
            if (parts.Length < 2) throw new ArgumentException("Missing target amount");
            if (!long.TryParse(parts[1], out _targetAmount)) throw new ArgumentException("Invalid amount format");
        }

        public string Execute()
        {
            var potentialTargets = GenerateIpRange();
            var victims = new List<(string Ip, long Funds, int Clients)>();

            foreach (var ip in potentialTargets)
            {
                try
                {
                    string rawBa = NetworkClient.SendRequest(ip, AppConfig.Port, "BA");
                    string rawBn = NetworkClient.SendRequest(ip, AppConfig.Port, "BN");

                    if (ParseResponse(rawBa, "BA", out long funds) && ParseResponse(rawBn, "BN", out long clients))
                    {
                        victims.Add((ip, funds, (int)clients));
                    }
                }
                catch
                {
                    // Node unreachable, skip
                }
            }

            var sortedVictims = victims.OrderByDescending(v => v.Funds).ToList();

            long currentSum = 0;
            int totalAffectedClients = 0;
            var targetIps = new List<string>();

            foreach (var victim in sortedVictims)
            {
                if (currentSum >= _targetAmount) break;

                currentSum += victim.Funds;
                totalAffectedClients += victim.Clients;
                targetIps.Add(victim.Ip);
            }

            if (currentSum == 0) return "RP Network poor, robbery impossible.";

            string ipList = string.Join(", ", targetIps);
            return $"RP To reach {_targetAmount} (found {currentSum}), rob banks: {ipList}. Total victims: {totalAffectedClients}.";
        }

        private bool ParseResponse(string response, string prefix, out long value)
        {
            value = 0;
            if (string.IsNullOrEmpty(response) || !response.StartsWith(prefix)) return false;

            var parts = response.Split(' ');
            return parts.Length > 1 && long.TryParse(parts[1], out value);
        }

        private List<string> GenerateIpRange()
        {
            // For classroom demo purposes, we scan localhost and a few potential peers
            return new List<string>
            {
                "127.0.0.1",
                AppConfig.IpAddress
            };
        }
    }
}