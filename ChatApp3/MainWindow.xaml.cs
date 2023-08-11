using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChatApp3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        private ConcurrentQueue<IPAddress> IPAddressesQueue;

        private ObservableCollection<string> messages;

        private string IP = "192.168.70.135";

        private const int BufferSize = 1024;

        private bool IsListening = true;

        private const int Port = 12000;

        private Thread listenerThreads;

        private Socket listenerSocket;

        public MainWindow()
        {
            InitializeComponent();
            messages = new ObservableCollection<string>();
            receiveListView.ItemsSource = messages;
            IPAddressesQueue = new();
        }

        private void Listener_Loaded(object sender, RoutedEventArgs e)
        {
            listenerThreads = new Thread(Listen);
            IsListening = true;
            listenerThreads.Start();
        }

        private void Listen()
        {
            try
            {
                IPAddress ipAddress = IPAddress.Parse(IP);
                listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                var ListenerEndPointVariable = new IPEndPoint(ipAddress, Port);
                listenerSocket.Bind(ListenerEndPointVariable);
                listenerSocket.Listen();

                while (IsListening)
                {
                    var clientSocket = listenerSocket.Accept();

                    byte[] buffer = new byte[BufferSize];

                    int bytesRead;

                    while ((bytesRead = clientSocket.Receive(buffer)) > 0)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                        var validatedIpEndPoint = Validate_Ip(buffer);

                        if(validatedIpEndPoint != null && !IPAddressesQueue.Contains(validatedIpEndPoint)) 
                        {
                            Connect(validatedIpEndPoint);
                        }

                        if(message.StartsWith("Disconnect:"))
                        {
                            var validateDisconnectionIP = Validate_Disconnection_Of_Ip(buffer);
                            IPAddressesQueue.TryDequeue(out validateDisconnectionIP);
                        }

                        Dispatcher.Invoke(() => messages.Add(message));
                    }

                    clientSocket.Close();
                }
            }
            catch (SocketException ex)
            {
                MessageBox.Show($"SocketException: {ex.Message}");
            }
        }

        private void Connect(IPAddress receivedIP)
        {
            foreach(var ipConnector in IPAddressesQueue)
            {
                var endPointMessage = Encoding.UTF8.GetBytes($"Join:{ipConnector};");
                Send_BroadCast(endPointMessage, receivedIP);
            }

            if (listenerSocket.LocalEndPoint is IPEndPoint myiep)
            {
                var myEndPointBytes = Encoding.UTF8.GetBytes($"Join:{myiep.Address};");
                Send_BroadCast(myEndPointBytes, receivedIP);
            }

            if (!IPAddressesQueue.Contains(receivedIP))
            {
                IPAddressesQueue.Enqueue(receivedIP);
            }
        }

        // Connect through UI
        private void Connect()
        {
            try
            {
                var ipAdd = IPTextBox.Text;
                var verifyIpAddress = IPAddress.TryParse(ipAdd, out var ipAddress);

                if(!verifyIpAddress)
                {
                    MessageBox.Show("Your ip is not correct, please check it once again");
                }

                var ipEndPoint = new IPEndPoint(ipAddress!, Port);

                if (!IPAddressesQueue.Contains(ipAddress))
                {
                    IPAddressesQueue.Enqueue(ipAddress!);
                }

                try
                {
                    Dispatcher.Invoke(() =>
                    {
                        Send_BroadCast(Encoding.UTF8.GetBytes($"Join:{IP};"), ipEndPoint.Address);
                    });
                }
                catch (SocketException ex)
                {
                    MessageBox.Show($"SocketException: {ex.Message}");
                }
            }
            catch (SocketException ex)
            {
                MessageBox.Show($"SocketException: {ex.Message}");
            }
        }

        private void Send_Message(byte[] buffer)
        {
            try
            {
                foreach (var ipAddresses in IPAddressesQueue)
                {
                    Send_BroadCast(buffer, ipAddresses);
                }
            }
            catch (SocketException ex)
            {
                MessageBox.Show($"SocketException: {ex.Message}");
            }
        }

        private void Send_BroadCast(byte[] buffer, IPAddress ipAdd)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ipAdd, Port);
            socket.Send(buffer);
            socket.Close();
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            Connect();
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            string message = MessageTextBox.Text;
            Send_Message(Encoding.UTF8.GetBytes(message));
            MessageTextBox.Text = "";
        }

        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            Send_Message(Encoding.UTF8.GetBytes($"Disconnect:{IP};"));
            IsListening = false;
            listenerSocket.Close();
        }

        public void Dispose()
        {
            listenerSocket.Dispose();
        }

        private static IPAddress? Validate_Ip(byte[] buffer)
        {
            string receivedIPAdd = Encoding.UTF8.GetString(buffer);

            if (receivedIPAdd.StartsWith("Join:"))
            {
                var splitIPStrings = receivedIPAdd.Split(':', ';');

                string ip = "";

                foreach (var str in splitIPStrings)
                {
                    if (str.Contains('.'))
                    {
                        ip = str;
                    }
                }

                var ipValidations = ip.Split(".");

                for (int i = 0; i < ipValidations.Length; i++)
                {
                    int ipChecks = int.Parse(ipValidations[i]);
                    if (ipChecks < 0 || ipChecks > 255)
                    {
                        return null;
                    }
                }

                return IPAddress.Parse(ip);
            }

            return null;
        }

        public static IPAddress? Validate_Disconnection_Of_Ip(byte[] buffer)
        {
            string receivedMessage = Encoding.UTF8.GetString(buffer);

            if (receivedMessage.StartsWith("Disconnect:"))
            {
                var splitIPStrings = receivedMessage.Split(':', ';');

                string ip = "";

                foreach (var str in splitIPStrings)
                {
                    if (str.Contains('.'))
                    {
                        ip = str;
                    }
                }

                return IPAddress.Parse(ip);
            }

            return null;
        }
    }
}
