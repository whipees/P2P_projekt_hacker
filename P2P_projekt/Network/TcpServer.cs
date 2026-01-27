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
                // Nasloucháme na všech IP adresách
                _listener = new TcpListener(IPAddress.Any, AppConfig.Port);
                _listener.Start();

                Logger.Instance.Log($"SERVER START: Naslouchám na portu {AppConfig.Port}");

                // Spustíme hlavní naslouchací smyčku
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
                Logger.Instance.Log("SERVER STOP: Server byl bezpečně ukončen.");
            }
            catch { /* Ignorovat chyby při vypínání */ }
        }

        private async Task ListenLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (_listener == null) break;

                    // Čekáme na klienta (PuTTY nebo jiný Node)
                    TcpClient client = await _listener.AcceptTcpClientAsync(token);

                    // Každého klienta řešíme v novém vlákně - NEBLOKUJE ostatní
                    _ = Task.Run(() => HandleClient(client), token);
                }
                catch (OperationCanceledException)
                {
                    break; // Normální vypnutí
                }
                catch (Exception ex)
                {
                    Logger.Instance.Error($"Loop Error: {ex.Message}");
                    // Krátká pauza, aby se log nezahltil, kdyby se něco zacyklilo
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
                    stream.ReadTimeout = 300000; // 5 minut timeout pro neaktivitu (aby spojení drželo)

                    // HLAVNÍ SMYČKA KOMUNIKACE S JEDNÍM KLIENTEM
                    // Čteme řádek po řádku, dokud se klient neodpojí
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        try
                        {
                            string request = line.Trim();
                            if (string.IsNullOrWhiteSpace(request)) continue; // Ignorovat prázdné Entery

                            Logger.Instance.Log($"[{clientIp}] Příkaz: {request}");

                            // 1. Zpracování příkazu
                            ICommand cmd = CommandFactory.Parse(request);
                            string response = cmd.Execute();

                            // 2. Odeslání odpovědi
                            writer.WriteLine(response); // WriteLine automaticky přidá \r\n

                            Logger.Instance.Log($"[{clientIp}] Odpověď: {response}");
                        }
                        catch (Exception innerEx)
                        {
                            // Pokud selže zpracování jednoho příkazu, spojení NEPADÁ
                            writer.WriteLine($"ER Interní chyba serveru: {innerEx.Message}");
                            Logger.Instance.Error($"Chyba zpracování příkazu: {innerEx.Message}");
                        }
                    }
                }
            }
            catch (IOException)
            {
                // Klient se odpojil (zavřel PuTTY) - to je normální, ne chyba
                Logger.Instance.Log($"Klient {clientIp} se odpojil.");
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Chyba spojení s {clientIp}: {ex.Message}");
            }
        }
    }
}