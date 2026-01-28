
using P2P_projekt.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2P_projekt.Commands
{
    public static class CommandFactory
    {
        public static ICommand Parse(string input)
        {
            // Základní ošetření prázdného vstupu
            if (string.IsNullOrWhiteSpace(input))
                return new ErrorCommand("Prázdný příkaz");

            try
            {
                string[] parts = input.Trim().Split(' ');
                if (parts.Length == 0) return new ErrorCommand("Neplatný formát");

                string code = parts[0].ToUpper();

                // Ošetření neexistujících příkazů
                switch (code)
                {
                    case "BC": return new BankCodeCommand();
                    case "AC": return new AccountCreateCommand();
                    case "BA": return new BankInfoCommand(true);
                    case "BN": return new BankInfoCommand(false);
                    case "RP": return new RobberyCommand(parts);
                    case "AD":
                    case "AW":
                    case "AB":
                    case "AR":
                        return HandleTransaction(code, parts, input);
                    default:
                        return new ErrorCommand($"Neznámý příkaz: {code}");
                }
            }
            catch (Exception ex)
            {
                // Kdyby cokoliv selhalo při parsování, vrátíme ER
                return new ErrorCommand($"Chyba formátu příkazu: {ex.Message}");
            }
        }

        private static ICommand HandleTransaction(string code, string[] parts, string fullCmd)
        {
            if (parts.Length < 2) throw new ArgumentException();

            string[] target = parts[1].Split('/');
            if (target.Length != 2) throw new ArgumentException();

            string accStr = target[0];
            string ip = target[1];

            // Proxy Logic
            if (ip != AppConfig.Settings.IpAddress && ip != "127.0.0.1" && ip != "0.0.0.0")
            {
                return new ProxyCommand(ip, fullCmd);
            }

            int accId = int.Parse(accStr);
            long amount = 0;
            if (parts.Length > 2) long.TryParse(parts[2], out amount);

            return code switch
            {
                "AD" => new DepositCommand(accId, amount),
                "AW" => new WithdrawCommand(accId, amount),
                "AB" => new BalanceCommand(accId),
                "AR" => new RemoveCommand(accId),
                _ => new ErrorCommand("Logic error")
            };
        }
    }
}
