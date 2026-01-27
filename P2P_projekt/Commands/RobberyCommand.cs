using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using P2P_projekt.Config;
using P2P_projekt.Network;
using P2P_projekt.Core;


namespace P2P_projekt.Commands
{
    public class RobberyCommand : ICommand
    {
        private readonly long _target;

        public RobberyCommand(string[] parts)
        {
            if (parts.Length < 2) throw new System.Exception("Target needed");
            _target = long.Parse(parts[1]);
        }

        public string Execute()
        {
            var potentialVictims = new List<string> { AppConfig.IpAddress, "127.0.0.1" };

            var results = new List<(string Ip, long Money, int Clients)>();

            foreach (var ip in potentialVictims)
            {
                try
                {
                    string ba = NetworkClient.SendRequest(ip, AppConfig.Port, "BA");
                    string bn = NetworkClient.SendRequest(ip, AppConfig.Port, "BN");

                    if (ba.StartsWith("BA") && bn.StartsWith("BN"))
                    {
                        long money = long.Parse(ba.Split(' ')[1]);
                        int clients = int.Parse(bn.Split(' ')[1]);
                        results.Add((ip, money, clients));
                    }
                }
                catch { }
            }

            var sorted = results.OrderByDescending(x => x.Money).ToList();
            long stolen = 0;
            int victims = 0;
            var targets = new List<string>();

            foreach (var node in sorted)
            {
                if (stolen >= _target) break;
                stolen += node.Money;
                victims += node.Clients;
                targets.Add(node.Ip);
            }

            return $"RP To get {_target}, rob: {string.Join(", ", targets)}. Total victims: {victims}";
        }
    }
}