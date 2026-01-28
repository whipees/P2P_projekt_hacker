using System.Collections.Generic;

namespace P2P_projekt.Core
{
    public interface ICommand
    {
        string Execute();
    }

    public interface IBankObserver
    {
        void Update(long totalFunds, int totalClients);
    }

    public interface IStorage
    {
        void Save(Dictionary<int, long> accounts);
        Dictionary<int, long> Load();
    }
}