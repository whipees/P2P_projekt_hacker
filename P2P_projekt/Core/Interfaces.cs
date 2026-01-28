using System.Collections.Generic;

namespace P2P_projekt.Core
{
    /// <summary>
    /// Defines a common interface for all command pattern implementations within the application.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Executes the logic associated with the specific command.
        /// </summary>
        /// <returns>A string response resulting from the command execution, typically formatted for network transmission.</returns>
        string Execute();
    }

    /// <summary>
    /// Defines an interface for observers that monitor changes in the bank's state.
    /// </summary>
    public interface IBankObserver
    {
        /// <summary>
        /// Updates the observer with the latest bank statistics.
        /// </summary>
        /// <param name="totalFunds">The total sum of balances across all accounts.</param>
        /// <param name="totalClients">The total number of unique accounts registered in the bank.</param>
        void Update(long totalFunds, int totalClients);
    }

    /// <summary>
    /// Defines a contract for persistent storage operations of bank account data.
    /// </summary>
    public interface IStorage
    {
        /// <summary>
        /// Persists the provided account data to the storage medium.
        /// </summary>
        /// <param name="accounts">A dictionary containing account IDs as keys and balances as values.</param>
        void Save(Dictionary<int, long> accounts);

        /// <summary>
        /// Loads account data from the storage medium.
        /// </summary>
        /// <returns>A dictionary containing the retrieved account data.</returns>
        Dictionary<int, long> Load();
    }
}