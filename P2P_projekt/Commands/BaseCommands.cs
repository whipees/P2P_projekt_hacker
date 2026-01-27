

namespace P2P_projekt.Commands
{
    using P2P_projekt.Config;
    using P2P_projekt.Network;
    using P2P_projekt.Core;
    using System;

        public class BankCodeCommand : ICommand
        {
            public string Execute() => $"BC {AppConfig.IpAddress}";
        }

        public class AccountCreateCommand : ICommand
        {
            public string Execute()
            {
                try
                {
                    // Calls Person B's code
                    int acc = BankEngine.Instance.CreateAccount();
                    return $"AC {acc}/{AppConfig.IpAddress}";
                }
                catch (Exception ex)
                {
                    return $"ER {ex.Message}";
                }
            }
        }

        public class DepositCommand : ICommand
        {
            private readonly int _acc;
            private readonly long _amt;
            public DepositCommand(int acc, long amt) { _acc = acc; _amt = amt; }

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

        public class WithdrawCommand : ICommand
        {
            private readonly int _acc;
            private readonly long _amt;
            public WithdrawCommand(int acc, long amt) { _acc = acc; _amt = amt; }

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

        public class BalanceCommand : ICommand
        {
            private readonly int _acc;
            public BalanceCommand(int acc) { _acc = acc; }

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

        public class RemoveCommand : ICommand
        {
            private readonly int _acc;
            public RemoveCommand(int acc) { _acc = acc; }

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

        public class BankInfoCommand : ICommand
        {
            private readonly bool _isAmount;
            public BankInfoCommand(bool isAmount) { _isAmount = isAmount; }

            public string Execute()
            {
                if (_isAmount) return $"BA {BankEngine.Instance.GetTotalFunds()}";
                return $"BN {BankEngine.Instance.GetClientCount()}";
            }
        }

        public class ProxyCommand : ICommand
        {
            private readonly string _targetIp;
            private readonly string _fullCommand;

            public ProxyCommand(string ip, string cmd)
            {
                _targetIp = ip;
                _fullCommand = cmd;
            }

            public string Execute()
            {
                return NetworkClient.SendRequest(_targetIp, AppConfig.Port, _fullCommand);
            }
        }

        public class ErrorCommand : ICommand
        {
            private readonly string _msg;
            public ErrorCommand(string msg) => _msg = msg;
            public string Execute() => $"ER {_msg}";
        }
    }

