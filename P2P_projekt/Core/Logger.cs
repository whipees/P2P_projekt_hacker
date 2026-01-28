using System;
using System.IO;

namespace P2P_projekt.Core
{
    /// <summary>
    /// A thread-safe Singleton class responsible for logging application events and errors to a local file.
    /// </summary>
    public sealed class Logger
    {
        private static readonly Lazy<Logger> _lazy = new(() => new Logger());

        /// <summary>
        /// Gets the Singleton instance of the <see cref="Logger"/>.
        /// </summary>
        public static Logger Instance => _lazy.Value;

        private readonly object _lock = new();
        private const string LogFile = "bank_node.log";

        /// <summary>
        /// Private constructor to prevent external instantiation, adhering to the Singleton pattern.
        /// </summary>
        private Logger() { }

        /// <summary>
        /// Appends a timestamped message to the log file in a thread-safe manner.
        /// </summary>
        /// <param name="message">The message string to be logged.</param>
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

        /// <summary>
        /// Logs a message specifically flagged as an error.
        /// </summary>
        /// <param name="message">The error description to be logged.</param>
        public void Error(string message) => Log($"ERROR: {message}");
    }
}