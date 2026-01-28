using P2P_projekt.Core;
using System;
using P2P_projekt.Config;

namespace P2P_projekt.Commands
{
    public static class CommandFactory
    {
        public static ICommand Parse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return new ErrorCommand("Empty command");

            try
            {
                string[] parts = input.Trim().Split(' ');
                if (parts.Length == 0) return new ErrorCommand(Localization.Get("ErrFormat"));

                string code = parts[0].ToUpper();

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
                        return new ErrorCommand($"Unknown command: {code}");
                }
            }
            catch (Exception ex)
            {
                return new ErrorCommand($"{Localization.Get("ErrFormat")}: {ex.Message}");
            }
        }

        private static ICommand HandleTransaction(string code, string[] parts, string fullCmd)
        {
            if (parts.Length < 2) throw new ArgumentException(Localization.Get("ErrFormat"));

            string[] target = parts[1].Split('/');
            if (target.Length != 2) throw new ArgumentException(Localization.Get("ErrFormat"));

            string accStr = target[0];
            string ip = target[1];

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
                _ => new ErrorCommand(Localization.Get("ErrFormat"))
            };
        }
    }
}