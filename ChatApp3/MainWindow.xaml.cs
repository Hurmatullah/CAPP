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
using ChatBackend;
using System.ComponentModel;

namespace ChatApp3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<string> messages;

        private bool IsListening = true;

        private Thread listenerThreads;

        private User user;

        public MainWindow()
        {
            InitializeComponent();
            user = new();
            messages = new ObservableCollection<string>();
            receiveListView.ItemsSource = messages;
        }

        private void LoadListener(object sender, RoutedEventArgs e)
        {
            listenerThreads = new Thread(Listen);
            IsListening = true;
            listenerThreads.Start();
        }

        private void Listen()
        {
            try
            {
                user.Listen();

                while (IsListening)
                {
                    var clientSocket = user.listenerSocket.Accept();
                    byte[] buffer = new byte[user.BufferSize];
                    int bytesRead;

                    while ((bytesRead = clientSocket.Receive(buffer)) > 0)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        var validatedIpEndPoint = user.ValidateIp(buffer);

                        if (validatedIpEndPoint != null && !user.ipAddressesQueue.Contains(validatedIpEndPoint))
                        {
                            user.Connect(validatedIpEndPoint);
                        }

                        if (message.StartsWith("Disconnect:"))
                        {
                            var validateDisconnectionIP = user.ValidateDisconnectionOfIp(buffer);
                            user.ipAddressesQueue.TryDequeue(out validateDisconnectionIP);
                        }

                        Dispatcher.Invoke(() => messages.Add(message));
                    }

                    clientSocket.Close();
                }
            }
            catch (SocketException ex)
            {
                MessageBox.Show($"SocketException: {ex.Message}");
                //MessageBox.Show("Socket disconnected successfully");
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error: {e.Message}");
            }
        }

        // Connect through UI

        private void Connect()
        {
            try
            {
                var ipAdd = IPTextBox.Text;
                var verifyIpAddress = IPAddress.TryParse(ipAdd, out var ipAddress);

                if (!verifyIpAddress)
                {
                    MessageBox.Show("Your ip is not correct, please check it once again");
                }

                var ipEndPoint = new IPEndPoint(ipAddress!, user.Port);

                if (!user.ipAddressesQueue.Contains(ipAddress))
                {
                    user.ipAddressesQueue.Enqueue(ipAddress!);
                }

                try
                {
                    Dispatcher.Invoke(() =>
                    {
                        user.SendBroadCast(Encoding.UTF8.GetBytes($"Join:{user.IP};"), ipEndPoint.Address);
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Exception: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception: {ex.Message}");
            }
        }

        private void ConnectClick(object sender, RoutedEventArgs e)
        {
            Connect();
        }

        private void SendClick(object sender, RoutedEventArgs e)
        {
            string message = MessageTextBox.Text;
            user.SendMessage(Encoding.UTF8.GetBytes(message));
            MessageTextBox.Text = "";
        }

        private void CloseClick(object sender, CancelEventArgs e)
        {
            user.SendMessage(Encoding.UTF8.GetBytes($"Disconnect:{user.IP};"));
            IsListening = false;
            user.Close();
            user.Dispose();
        }
    }
}
