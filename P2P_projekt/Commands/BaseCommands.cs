using P2P_projekt.Network;
using P2P_projekt.Core;
using System;
using P2P_projekt.Config;

namespace P2P_projekt.Commands
{
    /// <summary>
    /// Command to retrieve the bank's identification code (IP address).
    /// </summary>
    public class BankCodeCommand : ICommand
    {
        /// <summary>
        /// Executes the command to return the local bank's IP address.
        /// </summary>
        /// <returns>A string in the format "BC [IpAddress]".</returns>
        public string Execute() => $"BC {AppConfig.Settings.IpAddress}";
    }

    /// <summary>
    /// Command to create a new bank account within the local engine.
    /// </summary>
    public class AccountCreateCommand : ICommand
    {
        /// <summary>
        /// Executes the account creation process.
        /// </summary>
        /// <returns>A string with the new account number and IP, or an error message.</returns>
        public string Execute()
        {
            try
            {
                int acc = BankEngine.Instance.CreateAccount();
                return $"AC {acc}/{AppConfig.Settings.IpAddress}";
            }
            catch (Exception ex)
            {
                return $"ER {ex.Message}";
            }
        }
    }

    /// <summary>
    /// Command to deposit a specific amount into a bank account.
    /// </summary>
    public class DepositCommand : ICommand
    {
        private readonly int _acc;
        private readonly long _amt;

        /// <summary>
        /// Initializes a new instance of the <see cref="DepositCommand"/> class.
        /// </summary>
        /// <param name="acc">The account number.</param>
        /// <param name="amt">The amount to deposit.</param>
        public DepositCommand(int acc, long amt) { _acc = acc; _amt = amt; }

        /// <summary>
        /// Executes the deposit operation.
        /// </summary>
        /// <returns>"AD" on success, or an error message starting with "ER".</returns>
        public string Execute()
        {
            try
            {
                BankEngine.Instance.Deposit(_acc, _amt);
                return "AD";
            }
            catch (Exception ex) { return $"ER {ex.Message}"; }
        }
    }

    /// <summary>
    /// Command to withdraw a specific amount from a bank account.
    /// </summary>
    public class WithdrawCommand : ICommand
    {
        private readonly int _acc;
        private readonly long _amt;

        /// <summary>
        /// Initializes a new instance of the <see cref="WithdrawCommand"/> class.
        /// </summary>
        /// <param name="acc">The account number.</param>
        /// <param name="amt">The amount to withdraw.</param>
        public WithdrawCommand(int acc, long amt) { _acc = acc; _amt = amt; }

        /// <summary>
        /// Executes the withdrawal operation.
        /// </summary>
        /// <returns>"AW" on success, or an error message starting with "ER".</returns>
        public string Execute()
        {
            try
            {
                BankEngine.Instance.Withdraw(_acc, _amt);
                return "AW";
            }
            catch (Exception ex) { return $"ER {ex.Message}"; }
        }
    }

    /// <summary>
    /// Command to check the current balance of a bank account.
    /// </summary>
    public class BalanceCommand : ICommand
    {
        private readonly int _acc;

        /// <summary>
        /// Initializes a new instance of the <see cref="BalanceCommand"/> class.
        /// </summary>
        /// <param name="acc">The account number.</param>
        public BalanceCommand(int acc) { _acc = acc; }

        /// <summary>
        /// Executes the balance retrieval operation.
        /// </summary>
        /// <returns>A string in the format "AB [balance]", or an error message.</returns>
        public string Execute()
        {
            try
            {
                long bal = BankEngine.Instance.GetBalance(_acc);
                return $"AB {bal}";
            }
            catch (Exception ex) { return $"ER {ex.Message}"; }
        }
    }

    /// <summary>
    /// Command to remove an existing bank account.
    /// </summary>
    public class RemoveCommand : ICommand
    {
        private readonly int _acc;

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoveCommand"/> class.
        /// </summary>
        /// <param name="acc">The account number to be removed.</param>
        public RemoveCommand(int acc) { _acc = acc; }

        /// <summary>
        /// Executes the account removal operation.
        /// </summary>
        /// <returns>"AR" on success, or an error message starting with "ER".</returns>
        public string Execute()
        {
            try
            {
                BankEngine.Instance.RemoveAccount(_acc);
                return "AR";
            }
            catch (Exception ex) { return $"ER {ex.Message}"; }
        }
    }

    /// <summary>
    /// Command to retrieve general bank statistics such as total funds or client count.
    /// </summary>
    public class BankInfoCommand : ICommand
    {
        private readonly bool _isAmount;

        /// <summary>
        /// Initializes a new instance of the <see cref="BankInfoCommand"/> class.
        /// </summary>
        /// <param name="isAmount">If true, retrieves total funds (BA). If false, retrieves client count (BN).</param>
        public BankInfoCommand(bool isAmount) { _isAmount = isAmount; }

        /// <summary>
        /// Executes the info retrieval operation based on the <see cref="_isAmount"/> flag.
        /// </summary>
        /// <returns>A string starting with "BA" or "BN" followed by the value.</returns>
        public string Execute()
        {
            if (_isAmount) return $"BA {BankEngine.Instance.GetTotalFunds()}";
            return $"BN {BankEngine.Instance.GetClientCount()}";
        }
    }

    /// <summary>
    /// Command that acts as a proxy to forward requests to another node in the P2P network.
    /// </summary>
    public class ProxyCommand : ICommand
    {
        private readonly string _targetIp;
        private readonly string _fullCommand;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyCommand"/> class.
        /// </summary>
        /// <param name="ip">The target IP address of the remote node.</param>
        /// <param name="cmd">The raw command string to be forwarded.</param>
        public ProxyCommand(string ip, string cmd)
        {
            _targetIp = ip;
            _fullCommand = cmd;
        }

        /// <summary>
        /// Sends the command over the network to the target IP.
        /// </summary>
        /// <returns>The response string received from the remote node.</returns>
        public string Execute()
        {
            return NetworkClient.SendRequest(_targetIp, AppConfig.Settings.Port, _fullCommand);
        }
    }

    /// <summary>
    /// Command used to return a formatted error response.
    /// </summary>
    public class ErrorCommand : ICommand
    {
        private readonly string _msg;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorCommand"/> class.
        /// </summary>
        /// <param name="msg">The error message to be wrapped.</param>
        public ErrorCommand(string msg) => _msg = msg;

        /// <summary>
        /// Returns the error message in the protocol format.
        /// </summary>
        /// <returns>A string in the format "ER [message]".</returns>
        public string Execute() => $"ER {_msg}";
    }
}