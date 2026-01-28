using P2P_projekt.Core;
using System;
using P2P_projekt.Config;

namespace P2P_projekt.Commands
{
    /// <summary>
    /// Factory class responsible for parsing raw string input and creating the appropriate command objects.
    /// </summary>
    public static class CommandFactory
    {
        /// <summary>
        /// Parses a raw string input into a specific <see cref="ICommand"/> implementation.
        /// </summary>
        /// <param name="input">The raw command string received from a client or user.</param>
        /// <returns>A concrete instance of <see cref="ICommand"/> based on the command code.</returns>
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

        /// <summary>
        /// Handles the parsing and creation of transaction-related commands, including routing to proxy if necessary.
        /// </summary>
        /// <param name="code">The command operation code (e.g., AD, AW, AB, AR).</param>
        /// <param name="parts">The split parts of the original command string.</param>
        /// <param name="fullCmd">The full original command string used for proxy forwarding.</param>
        /// <returns>A command instance for local execution or a <see cref="ProxyCommand"/> for remote execution.</returns>
        /// <exception cref="ArgumentException">Thrown when the input format is invalid.</exception>
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