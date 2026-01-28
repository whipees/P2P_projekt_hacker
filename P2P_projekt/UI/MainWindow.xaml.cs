using System;
using System.Windows;
using System.Windows.Media;
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
                TxtIp.Text = $"{Localization.Get("Ip")}: {AppConfig.IpAddress} | Port: {AppConfig.Port}";
                RefreshTexts();

                BankEngine.Instance.Attach(this);
                // Initial update
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
                // Update Numbers
                TxtFunds.Text = $"${totalFunds}";
                TxtClients.Text = totalClients.ToString();

                // Update Status Indicator based on Engine State
                bool isOnline = BankEngine.Instance.IsOnline;
                if (isOnline)
                {
                    TxtStatus.Text = "● " + Localization.Get("StatusOnline");
                    TxtStatus.Foreground = new SolidColorBrush(Color.FromRgb(46, 125, 50)); // Green
                    StatusBorder.Background = new SolidColorBrush(Color.FromRgb(232, 245, 233)); // Light Green
                }
                else
                {
                    TxtStatus.Text = "● " + Localization.Get("StatusOffline");
                    TxtStatus.Foreground = new SolidColorBrush(Color.FromRgb(198, 40, 40)); // Red
                    StatusBorder.Background = new SolidColorBrush(Color.FromRgb(255, 235, 238)); // Light Red
                }
            });
        }

        private void BtnLang_Click(object sender, RoutedEventArgs e)
        {
            Localization.ToggleLanguage();
            RefreshTexts();
            // Trigger update to refresh status text
            Update(BankEngine.Instance.GetTotalFunds(), BankEngine.Instance.GetClientCount());
        }

        private void RefreshTexts()
        {
            LblFunds.Text = Localization.Get("Funds");
            LblClients.Text = Localization.Get("Clients");
            BtnStop.Content = Localization.Get("Shutdown");
            TxtIp.Text = $"{Localization.Get("Ip")}: {AppConfig.IpAddress} | Port: {AppConfig.Port}";
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            _server?.Stop();
            Application.Current.Shutdown();
        }
    }
}