using System;
using System.Collections.Generic;
using System.Linq;
using P2P_projekt.Data;

namespace P2P_projekt.Core
{
    public sealed class BankEngine
    {
        private static readonly Lazy<BankEngine> _lazy = new(() => new BankEngine());
        public static BankEngine Instance => _lazy.Value;

        private readonly Dictionary<int, long> _accounts;
        private readonly object _lock = new();
        private readonly List<IBankObserver> _observers = new();
        private readonly IStorage _storage;

        private BankEngine()
        {
            _storage = new StorageChain();
            _accounts = _storage.Load();
        }

        public void Attach(IBankObserver observer)
        {
            lock (_lock)
            {
                if (!_observers.Contains(observer))
                {
                    _observers.Add(observer);
                }
            }
        }

        private void Notify()
        {
            long funds;
            int clients;
            lock (_lock)
            {
                funds = _accounts.Values.Sum();
                clients = _accounts.Count;
            }

            foreach (var observer in _observers)
            {
                observer.Update(funds, clients);
            }
        }

        public int CreateAccount()
        {
            lock (_lock)
            {
                var rnd = new Random();
                int newAcc;
                do
                {
                    newAcc = rnd.Next(10000, 99999);
                } while (_accounts.ContainsKey(newAcc));

                _accounts[newAcc] = 0;
                _storage.Save(_accounts);
                Logger.Instance.Log($"Account created: {newAcc}");
                Notify();
                return newAcc;
            }
        }

        public void Deposit(int accountId, long amount)
        {
            lock (_lock)
            {
                if (!_accounts.ContainsKey(accountId)) throw new Exception("Account not found");
                if (amount < 0) throw new Exception("Negative amount");

                _accounts[accountId] += amount;
                _storage.Save(_accounts);
                Logger.Instance.Log($"Deposit {amount} to {accountId}");
                Notify();
            }
        }

        public void Withdraw(int accountId, long amount)
        {
            lock (_lock)
            {
                if (!_accounts.ContainsKey(accountId)) throw new Exception("Account not found");
                if (_accounts[accountId] < amount) throw new Exception("Insufficient funds");

                _accounts[accountId] -= amount;
                _storage.Save(_accounts);
                Logger.Instance.Log($"Withdraw {amount} from {accountId}");
                Notify();
            }
        }

        public long GetBalance(int accountId)
        {
            lock (_lock)
            {
                if (!_accounts.ContainsKey(accountId)) throw new Exception("Account not found");
                return _accounts[accountId];
            }
        }

        public void RemoveAccount(int accountId)
        {
            lock (_lock)
            {
                if (!_accounts.ContainsKey(accountId)) throw new Exception("Account not found");
                if (_accounts[accountId] != 0) throw new Exception("Account not empty");

                _accounts.Remove(accountId);
                _storage.Save(_accounts);
                Logger.Instance.Log($"Account removed: {accountId}");
                Notify();
            }
        }

        public long GetTotalFunds()
        {
            lock (_lock)
            {
                return _accounts.Values.Sum();
            }
        }

        public int GetClientCount()
        {
            lock (_lock)
            {
                return _accounts.Count;
            }
        }
    }
}