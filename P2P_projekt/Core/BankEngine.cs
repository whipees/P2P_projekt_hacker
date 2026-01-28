using System;
using System.Collections.Generic;
using System.Linq;
using P2P_projekt.Data;

namespace P2P_projekt.Core
{
    /// <summary>
    /// Core engine of the bank, implemented as a thread-safe Singleton.
    /// Manages accounts, transactions, and notifies observers about state changes.
    /// </summary>
    public sealed class BankEngine
    {
        private static readonly Lazy<BankEngine> _lazy = new(() => new BankEngine());

        /// <summary>
        /// Gets the Singleton instance of the <see cref="BankEngine"/>.
        /// </summary>
        public static BankEngine Instance => _lazy.Value;

        private readonly Dictionary<int, long> _accounts;
        private readonly object _lock = new();
        private readonly List<IBankObserver> _observers = new();
        private readonly IStorage _storage;

        /// <summary>
        /// Gets or sets a value indicating whether the bank's network node is online.
        /// </summary>
        public bool IsOnline { get; set; } = false;

        /// <summary>
        /// Private constructor for the Singleton pattern. 
        /// Initializes the storage mechanism and loads existing account data.
        /// </summary>
        private BankEngine()
        {
            _storage = new StorageChain();
            _accounts = _storage.Load();
        }

        /// <summary>
        /// Registers a new observer to receive updates on bank status changes.
        /// </summary>
        /// <param name="observer">The observer to be attached.</param>
        public void Attach(IBankObserver observer)
        {
            lock (_lock)
            {
                if (!_observers.Contains(observer)) _observers.Add(observer);
            }
        }

        /// <summary>
        /// Calculates current bank statistics and notifies all attached observers.
        /// </summary>
        public void Notify()
        {
            long funds;
            int clients;
            lock (_lock)
            {
                funds = _accounts.Values.Sum();
                clients = _accounts.Count;
            }
            foreach (var observer in _observers) observer.Update(funds, clients);
        }

        /// <summary>
        /// Updates the online status of the bank and triggers a notification.
        /// </summary>
        /// <param name="online">The new online status.</param>
        public void SetStatus(bool online)
        {
            IsOnline = online;
            Notify();
        }

        /// <summary>
        /// Creates a new bank account with a unique random 5-digit ID.
        /// </summary>
        /// <returns>The newly created account ID.</returns>
        public int CreateAccount()
        {
            lock (_lock)
            {
                var rnd = new Random();
                int newAcc;
                do { newAcc = rnd.Next(10000, 99999); } while (_accounts.ContainsKey(newAcc));

                _accounts[newAcc] = 0;
                _storage.Save(_accounts);
                Logger.Instance.Log($"Account created: {newAcc}");
                Notify();
                return newAcc;
            }
        }

        /// <summary>
        /// Deposits a specific amount into an existing account.
        /// </summary>
        /// <param name="accountId">The ID of the target account.</param>
        /// <param name="amount">The non-negative amount to deposit.</param>
        /// <exception cref="Exception">Thrown if account is not found or amount is negative.</exception>
        public void Deposit(int accountId, long amount)
        {
            lock (_lock)
            {
                if (!_accounts.ContainsKey(accountId)) throw new Exception(Localization.Get("ErrAccount"));
                if (amount < 0) throw new Exception(Localization.Get("ErrFormat"));

                _accounts[accountId] += amount;
                _storage.Save(_accounts);
                Notify();
            }
        }

        /// <summary>
        /// Withdraws a specific amount from an existing account if funds are sufficient.
        /// </summary>
        /// <param name="accountId">The ID of the source account.</param>
        /// <param name="amount">The amount to withdraw.</param>
        /// <exception cref="Exception">Thrown if account is not found or funds are insufficient.</exception>
        public void Withdraw(int accountId, long amount)
        {
            lock (_lock)
            {
                if (!_accounts.ContainsKey(accountId)) throw new Exception(Localization.Get("ErrAccount"));
                if (_accounts[accountId] < amount) throw new Exception(Localization.Get("ErrFunds"));

                _accounts[accountId] -= amount;
                _storage.Save(_accounts);
                Notify();
            }
        }

        /// <summary>
        /// Retrieves the current balance of a specific account.
        /// </summary>
        /// <param name="accountId">The ID of the account.</param>
        /// <returns>The current balance as a long.</returns>
        /// <exception cref="Exception">Thrown if account is not found.</exception>
        public long GetBalance(int accountId)
        {
            lock (_lock)
            {
                if (!_accounts.ContainsKey(accountId)) throw new Exception(Localization.Get("ErrAccount"));
                return _accounts[accountId];
            }
        }

        /// <summary>
        /// Deletes an existing account from the system if its balance is zero.
        /// </summary>
        /// <param name="accountId">The ID of the account to remove.</param>
        /// <exception cref="Exception">Thrown if account is not found or account still has funds.</exception>
        public void RemoveAccount(int accountId)
        {
            lock (_lock)
            {
                if (!_accounts.ContainsKey(accountId)) throw new Exception(Localization.Get("ErrAccount"));
                if (_accounts[accountId] != 0) throw new Exception(Localization.Get("ErrNotEmpty"));

                _accounts.Remove(accountId);
                _storage.Save(_accounts);
                Notify();
            }
        }

        /// <summary>
        /// Returns the sum of all balances across all accounts.
        /// </summary>
        /// <returns>Total funds in the bank.</returns>
        public long GetTotalFunds() { lock (_lock) return _accounts.Values.Sum(); }

        /// <summary>
        /// Returns the total number of registered accounts.
        /// </summary>
        /// <returns>Client count.</returns>
        public int GetClientCount() { lock (_lock) return _accounts.Count; }
    }
}