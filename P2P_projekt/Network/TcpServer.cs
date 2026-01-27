using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using P2P_projekt.Commands;
using P2P_projekt.Config;
using P2P_projekt.Core; // Potřeba pro Logger

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
                // IPAddress.Any znamená, že posloucháme na všech síťových kartách PC
                _listener = new TcpListener(IPAddress.Any, AppConfig.Port);
                _listener.Start();

                Logger.Instance.Log($"Server started on port {AppConfig.Port}");

                // Spustíme smyčku v pozadí (Fire and forget), aby UI nezamrzlo
                Task.Run(() => ListenLoop(_cts.Token));
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Server start failed: {ex.Message}");
            }
        }

        public void Stop()
        {
            _cts?.Cancel();
            _listener?.Stop();
            Logger.Instance.Log("Server stopped.");
        }

        private async Task ListenLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (_listener == null) break;

                    // Asynchronní čekání na klienta
                    TcpClient client = await _listener.AcceptTcpClientAsync(token);

                    // Každého klienta obsloužíme v novém Tasku (paralelní zpracování)
                    _ = Task.Run(() => HandleClient(client), token);
                }
                catch
                {
                    // Listener byl zastaven nebo došlo k chybě socketu
                    break;
                }
            }
        }

        private void HandleClient(TcpClient client)
        {
            try
            {
                using (client)
                using (NetworkStream stream = client.GetStream())
                {
                    // Nastavení timeoutů dle zadání
                    stream.ReadTimeout = AppConfig.Timeout;
                    stream.WriteTimeout = AppConfig.Timeout;

                    byte[] buffer = new byte[1024];

                    // Čtení dat
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);

                    if (bytesRead > 0)
                    {
                        string request = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                        Logger.Instance.Log($"Received: {request}");

                        // Zpracování příkazu přes Command Factory
                        ICommand cmd = CommandFactory.Parse(request);
                        string response = cmd.Execute();

                        // Odeslání odpovědi + nový řádek (podle zadání)
                        byte[] respBytes = Encoding.UTF8.GetBytes(response + "\n");
                        stream.Write(respBytes, 0, respBytes.Length);

                        Logger.Instance.Log($"Sent: {response}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Chyba při komunikaci s jedním klientem nesmí shodit server
                Logger.Instance.Error($"Client handling error: {ex.Message}");
            }
        }
    }
}