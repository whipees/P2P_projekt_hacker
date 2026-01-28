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
    public class TcpServer
    {
        private TcpListener? _listener;
        private CancellationTokenSource? _cts;

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
                    stream.ReadTimeout = 300000; // 5 mins

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