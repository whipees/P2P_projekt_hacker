
using System.IO;

namespace P2PBankNode.Core
{
    public sealed class Logger
    {
        private static readonly Lazy<Logger> _lazy = new(() => new Logger());
        public static Logger Instance => _lazy.Value;
        private readonly object _lock = new();
        private const string LogFile = "bank_node.log";

        public void Log(string message)
        {
            lock (_lock)
            {
                try
                {
                    string entry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
                    File.AppendAllText(LogFile, entry + Environment.NewLine);
                }
                catch { }
            }
        }

        public void Error(string message) => Log($"ERROR: {message}");
    }
}