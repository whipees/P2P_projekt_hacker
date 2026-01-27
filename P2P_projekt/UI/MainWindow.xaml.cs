using System;
using System.Windows;
using P2P_projekt.Core;
using P2P_projekt.Network;
using P2P_projekt.Config;

namespace P2P_projekt
{
    public partial class MainWindow : Window, IBankObserver
    {
        private readonly TcpServer _server;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                TxtIp.Text = $"IP: {AppConfig.IpAddress} | Port: {AppConfig.Port}";

                BankEngine.Instance.Attach(this);
                Update(BankEngine.Instance.GetTotalFunds(), BankEngine.Instance.GetClientCount());

                _server = new TcpServer();
                _server.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Startup Error: {ex.Message}");
            }
        }

        public void Update(long totalFunds, int totalClients)
        {
            Dispatcher.Invoke(() =>
            {
                TxtFunds.Text = $"${totalFunds}";
                TxtClients.Text = totalClients.ToString();
            });
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            if (_server != null) _server.Stop();
            Application.Current.Shutdown();
        }
    }
}