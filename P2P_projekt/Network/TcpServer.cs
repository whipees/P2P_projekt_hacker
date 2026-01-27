using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using P2P_projekt.Commands;
using P2P_projekt.Config;

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
                _listener = new TcpListener(IPAddress.Any, AppConfig.Port);
                _listener.Start();

                // Fire and forget task
                Task.Run(() => ListenLoop(_cts.Token));
            }
            catch (Exception)
            {
                // Person B will handle logging errors here
            }
        }

        public void Stop()
        {
            _cts?.Cancel();
            _listener?.Stop();
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
                catch { break; }
            }
        }

        private void HandleClient(TcpClient client)
        {
            try
            {
                using (client)
                using (NetworkStream stream = client.GetStream())
                {
                    stream.ReadTimeout = AppConfig.Timeout;
                    stream.WriteTimeout = AppConfig.Timeout;

                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);

                    if (bytesRead > 0)
                    {
                        string request = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                        ICommand cmd = CommandFactory.Parse(request);
                        string response = cmd.Execute();

                        byte[] respBytes = Encoding.UTF8.GetBytes(response + "\n");
                        stream.Write(respBytes, 0, respBytes.Length);
                    }
                }
            }
            catch
            {
                // Connection errors ignored to prevent server crash
            }
        }
    }
}
