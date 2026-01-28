using System;
using System.Windows;
using System.Windows.Media;
using P2P_projekt.Core;
using P2P_projekt.Network;
using P2P_projekt.Config;

namespace P2P_projekt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml. 
    /// Acts as a UI observer for the BankEngine and manages the primary lifecycle of the TCP server.
    /// </summary>
    public partial class MainWindow : Window, IBankObserver
    {
        private readonly TcpServer _server;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// Performs configuration initialization, UI setup, and starts the TCP server.
        /// </summary>
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

        /// <summary>
        /// Updates the UI elements with current bank statistics. 
        /// This method is called by the <see cref="BankEngine"/> via the observer pattern.
        /// </summary>
        /// <param name="totalFunds">The total amount of funds across all accounts.</param>
        /// <param name="totalClients">The total number of active accounts.</param>
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
                    TxtStatus.Foreground = new SolidColorBrush(Color.FromRgb(46, 125, 50));
                }
                else
                {
                    TxtStatus.Text = "● " + P2P_projekt.Core.Localization.Get("StatusOffline");
                    TxtStatus.Foreground = new SolidColorBrush(Color.FromRgb(198, 40, 40));
                }
            });
        }

        /// <summary>
        /// Handles the language toggle button click event.
        /// Swaps the current localization and refreshes all UI text elements.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void BtnLang_Click(object sender, RoutedEventArgs e)
        {
            P2P_projekt.Core.Localization.ToggleLanguage();
            RefreshTexts();
            Update(BankEngine.Instance.GetTotalFunds(), BankEngine.Instance.GetClientCount());
        }

        /// <summary>
        /// Updates all static text labels in the UI using the current localization settings.
        /// </summary>
        private void RefreshTexts()
        {
            TxtIp.Text = $"{P2P_projekt.Core.Localization.Get("Ip")}: {AppConfig.Settings.IpAddress} | Port: {AppConfig.Settings.Port}";
            LblFunds.Text = P2P_projekt.Core.Localization.Get("Funds");
            LblClients.Text = P2P_projekt.Core.Localization.Get("Clients");
            BtnStop.Content = P2P_projekt.Core.Localization.Get("Shutdown");
        }

        /// <summary>
        /// Handles the shutdown button click event. 
        /// Stops the TCP server and exits the application.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            _server?.Stop();
            Application.Current.Shutdown();
        }
    }
}