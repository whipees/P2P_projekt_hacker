using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using P2P_projekt.Commands;
using P2P_projekt.Config;
using P2P_projekt.Core;

namespace P2P_projekt.Network
{
    /// <summary>
    /// Represents a multi-threaded TCP server that listens for incoming P2P bank commands 
    /// and dispatches them to the command factory for execution.
    /// </summary>
    public class TcpServer
    {
        private TcpListener? _listener;
        private CancellationTokenSource? _cts;

        /// <summary>
        /// Initializes and starts the TCP listener on the configured port.
        /// Updates the bank engine status and begins the asynchronous listening loop.
        /// </summary>
        public void Start()
        {
            try
            {
                _cts = new CancellationTokenSource();
                _listener = new TcpListener(IPAddress.Any, AppConfig.Settings.Port);
                _listener.Start();
                BankEngine.Instance.SetStatus(true);
                Logger.Instance.Log($"SERVER START: Listening on port {AppConfig.Settings.Port}");

                Task.Run(() => ListenLoop(_cts.Token));
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"CRITICAL SERVER ERROR: {ex.Message}");
            }
        }

        /// <summary>
        /// Gracefully stops the TCP server, cancels pending operations, 
        /// and updates the bank engine status to offline.
        /// </summary>
        public void Stop()
        {
            try
            {
                _cts?.Cancel();
                _listener?.Stop();
                BankEngine.Instance.SetStatus(false);
                Logger.Instance.Log("SERVER STOP: Server shut down safely.");
            }
            catch { }
        }

        /// <summary>
        /// Asynchronous loop that continuously accepts incoming TCP client connections
        /// until a cancellation is requested.
        /// </summary>
        /// <param name="token">Cancellation token to stop the loop.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ListenLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (_listener == null) break;

                    TcpClient client = await _listener.AcceptTcpClientAsync(token);

                    _ = Task.Run(() => HandleClient(client), token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Logger.Instance.Error($"Loop Error: {ex.Message}");
                    await Task.Delay(100);
                }
            }
        }

        /// <summary>
        /// Handles communication with a connected TCP client. 
        /// Reads incoming command lines, processes them via <see cref="CommandFactory"/>, 
        /// and sends back the resulting response.
        /// </summary>
        /// <param name="client">The connected <see cref="TcpClient"/> instance.</param>
        private void HandleClient(TcpClient client)
        {
            string clientIp = "Unknown";
            try
            {
                if (client.Client.RemoteEndPoint is IPEndPoint endPoint)
                {
                    clientIp = endPoint.Address.ToString();
                }

                Logger.Instance.Log($"Client Connected: {clientIp}");

                using (client)
                using (NetworkStream stream = client.GetStream())
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
                {
                    stream.ReadTimeout = 300000;

                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        try
                        {
                            string request = line.Trim();
                            if (string.IsNullOrWhiteSpace(request)) continue;

                            Logger.Instance.Log($"[{clientIp}] CMD: {request}");

                            ICommand cmd = CommandFactory.Parse(request);
                            string response = cmd.Execute();

                            writer.WriteLine(response);

                            Logger.Instance.Log($"[{clientIp}] RESP: {response}");
                        }
                        catch (Exception innerEx)
                        {
                            writer.WriteLine($"ER Internal Server Error: {innerEx.Message}");
                            Logger.Instance.Error($"Command Processing Error: {innerEx.Message}");
                        }
                    }
                }
            }
            catch (IOException)
            {
                Logger.Instance.Log($"Client {clientIp} disconnected.");
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Connection error with {clientIp}: {ex.Message}");
            }
        }
    }
}