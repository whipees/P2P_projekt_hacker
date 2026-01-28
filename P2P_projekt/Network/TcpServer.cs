using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using P2P_projekt.Commands;
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
                Logger.Instance.Log($"SERVER START: Naslouchám na portu {AppConfig.Settings.Port}");

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
                Logger.Instance.Log("SERVER STOP: Server byl bezpečně ukončen.");
            }
            catch {  }
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
            string clientIp = "Neznámý";
            try
            {
                if (client.Client.RemoteEndPoint is IPEndPoint endPoint)
                {
                    clientIp = endPoint.Address.ToString();
                }

                Logger.Instance.Log($"Připojen klient: {clientIp}");

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

                            Logger.Instance.Log($"[{clientIp}] Příkaz: {request}");

                            ICommand cmd = CommandFactory.Parse(request);
                            string response = cmd.Execute();

                            writer.WriteLine(response); 

                            Logger.Instance.Log($"[{clientIp}] Odpověď: {response}");
                        }
                        catch (Exception innerEx)
                        {
                            writer.WriteLine($"ER Interní chyba serveru: {innerEx.Message}");
                            Logger.Instance.Error($"Chyba zpracování příkazu: {innerEx.Message}");
                        }
                    }
                }
            }
            catch (IOException)
            {
                Logger.Instance.Log($"Klient {clientIp} se odpojil.");
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Chyba spojení s {clientIp}: {ex.Message}");
            }
        }
    }
}