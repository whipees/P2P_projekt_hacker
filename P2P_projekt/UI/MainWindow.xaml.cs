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
                AppConfig.Initialize();

                RefreshTexts();
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

                bool isOnline = BankEngine.Instance.IsOnline;
                if (isOnline)
                {
                    TxtStatus.Text = "● " + P2P_projekt.Core.Localization.Get("StatusOnline");
                    TxtStatus.Foreground = new SolidColorBrush(Color.FromRgb(46, 125, 50)); // Zelená
                }
                else
                {
                    TxtStatus.Text = "● " + P2P_projekt.Core.Localization.Get("StatusOffline");
                    TxtStatus.Foreground = new SolidColorBrush(Color.FromRgb(198, 40, 40)); // Červená
                }
            });
        }

        private void BtnLang_Click(object sender, RoutedEventArgs e)
        {
            P2P_projekt.Core.Localization.ToggleLanguage();
            RefreshTexts();
            Update(BankEngine.Instance.GetTotalFunds(), BankEngine.Instance.GetClientCount());
        }

        private void RefreshTexts()
        {
            TxtIp.Text = $"{P2P_projekt.Core.Localization.Get("Ip")}: {AppConfig.Settings.IpAddress} | Port: {AppConfig.Settings.Port}";
            LblFunds.Text = P2P_projekt.Core.Localization.Get("Funds");
            LblClients.Text = P2P_projekt.Core.Localization.Get("Clients");
            BtnStop.Content = P2P_projekt.Core.Localization.Get("Shutdown");
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            _server?.Stop();
            Application.Current.Shutdown();
        }
    }
}