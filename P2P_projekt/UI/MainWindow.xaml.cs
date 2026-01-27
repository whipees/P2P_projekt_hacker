using System.Windows;
using P2P_projekt.Config;
using P2P_projekt.Core;
using P2PBankNode.Config;
using P2PBankNode.Core;
using P2PBankNode.Network;

namespace P2PBankNode
{
    public partial class MainWindow : Window, IBankObserver
    {
        private readonly TcpServer _server;

        public MainWindow()
        {
            InitializeComponent();
            TxtIp.Text = $"IP: {AppConfig.IpAddress} | Port: {AppConfig.Port}";

            BankEngine.Instance.Attach(this);

            _server = new TcpServer();
            _server.Start();

            Update(BankEngine.Instance.GetTotalFunds(), BankEngine.Instance.GetClientCount());
        }

        public void Update(long totalFunds, int totalClients)
        {
            Dispatcher.Invoke(() =>
            {
                TxtFunds.Text = $"Total Funds: ${totalFunds}";
                TxtClients.Text = $"Clients: {totalClients}";
            });
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            _server.Stop();
            Application.Current.Shutdown();
        }
    }
}