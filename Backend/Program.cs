using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;

namespace Backend
{
    internal class Program
    {
        private string ServerIpAddress; // IP address of the listener server
        private int ServerPort; // Port number of the listener server

        private Socket listenerSocket;
        private IPEndPoint EndPointVariable;

        private string IpCurrent = "192.168.70.135";
        private const int ListenerPort = 12000; // Port number to listen on
        private const int BufferSize = 1024; // Buffer size for receiving messages

        private Thread listenerThread;
        private bool isListening = true;
        private ObservableCollection<string> messages;
        private ConcurrentBag<IPAddress> messagesQueue;
        private List<Socket> socketList;
        private void ListenForMessages()
        {
            try
            {
                Debug.WriteLine("Listener is started as well");
                IPAddress ipAddress = IPAddress.Parse(IpCurrent);
                listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                var ListenerEndPointVariable = new IPEndPoint(ipAddress, ListenerPort);
                listenerSocket.Bind(ListenerEndPointVariable);
                listenerSocket.Listen();

                while (isListening)
                {
                    //using (Socket clientSocket = listenerSocket.Accept())
                    //{
                    var clientSocket = listenerSocket.Accept();
                    //IPEndPoint clientEndpoint = (IPEndPoint) clientSocket.RemoteEndPoint;
                    //IPAddress clientIP = clientEndpoint.Address;
                    //int clientPort = clientEndpoint.Port;
                    //messagesQueue.Add(clientEndpoint);
                    //Debug.WriteLine("Added");
                    //Debug.WriteLine("Client connected from: {0}:{1}", clientIP, clientPort);

                    byte[] buffer = new byte[BufferSize];
                    int bytesRead;

                    if (clientSocket.Connected)
                    {
                        //if (!messagesQueue.Contains(IPAddress.Parse(Encoding.UTF8.GetString(buffer))))
                        //{
                        //    //messagesQueue.Add((IPAddress)clientSocket.RemoteEndPoint);
                        //    Debug.WriteLine("Hello from the queue: {0}", IPAddress.Parse(Encoding.UTF8.GetString(buffer)));
                        //    Debug.WriteLine(messagesQueue.Count);
                        //}

                        Debug.WriteLine("Yeah connected");
                    }

                    while ((bytesRead = clientSocket.Receive(buffer)) > 0)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                        var ipEndPoint = ParseIpAndPort(buffer);
                        Debug.Write($"Finally: {ipEndPoint}");

                        if (ipEndPoint != null && !messagesQueue.Contains(ipEndPoint))
                        {
                            ConnectNew(ipEndPoint);
                        }

                        //if (IPAddress.TryParse(message, out IPAddress ipcheck))
                        //{
                        //    Debug.WriteLine($"The is the IP: {ipcheck.GetType()}");
                        //    Debug.WriteLine("Above is the type of ipCheck");
                        //    Debug.WriteLine(ipcheck);
                        //    if (ipcheck != null && !messagesQueue.Contains(ipcheck))
                        //    {
                        //        ConnectToServer(ipcheck);
                        //        messagesQueue.Add(ipcheck);
                        //        Debug.WriteLine("stored and called that function");
                        //        Debug.WriteLine(messagesQueue.Count);
                        //    }
                        //}


                        //List<IPAddress> ipAddresses = new List<IPAddress>();
                        //foreach (string ipAddressStr in message.Split(','))
                        //{
                        //    Debug.WriteLine("Hello {0}", ipAddressStr);
                        //    Debug.WriteLine(ipAddressStr.GetType());
                        //    //IPAddress ipAddress1;
                        //    //if (!IPAddress.TryParse(ipAddressStr, out ipAddress1))
                        //    //{
                        //    //    Debug.WriteLine("Invalid IP address received: {0}", message);
                        //    //    break;
                        //    //}
                        //    //ipAddresses.Add(ipAddress);
                        //}
                        App.Current.Dispatcher.Invoke(() => messages.Add(message));
                        Application
                    }
                    //}

                    clientSocket.Close();
                }
            }
            catch (SocketException ex)
            {
                //MessageBox.Show($"SocketException: {ex.Message}");
                Debug.WriteLine($"SocketException: {ex.Message}");
            }
        }

        //private void ConnectToServer(IPAddress iphh)
        //{
        //    //ServerIpAddress = IPTextBox.Text.ToString();
        //    try
        //    {
        //        var ipAddress = Dispatcher.Invoke(() => IPTextBox.Text.ToString());
        //        ServerIpAddress = ipAddress;
        //        //var iAddress = connectionIEP.Address;
        //        //var iPort = connectionIEP.Port;
        //        //EndPointVariable = new IPEndPoint(IPAddress.Parse(ServerIpAddress), ServerPort);
        //        EndPointVariable = new IPEndPoint(iphh, ServerPort);

        //        //if (!messagesQueue.Contains(EndPointVariable.Address))
        //        //{
        //        //    messagesQueue.Add(EndPointVariable.Address);
        //        //    Debug.WriteLine("The ip that is stored is: {0}", EndPointVariable.Address);
        //        //    Debug.WriteLine("The number of stored ip is: {0}", messagesQueue.Count);
        //        //    Debug.WriteLine("This ip and Port werent in the queue, so we added");
        //        //}
        //        try
        //        {
        //            Dispatcher.Invoke(() =>
        //            {
        //                //if(messagesQueue.Count == 1)
        //                //{
        //                //    var endPointJan = Encoding.UTF8.GetBytes("");
        //                //    SendIndividual(endPointJan, EndPointVariable);
        //                //}
        //                //else
        //                //{

        //                //foreach (var ipMessageDefault in messagesQueue)
        //                //    {
        //                //        var endPointJan = Encoding.UTF8.GetBytes($"{ipMessageDefault.Address}");
        //                //        Debug.WriteLine(ipMessageDefault);
        //                //        SendBroadCast(endPointJan, ipMessageDefault);
        //                //    }
        //                //}
        //                string ipEndpointsMessage = GetIpEndpointsMessage(messagesQueue.ToList());
        //                Debug.WriteLine("Hello {0}", ipEndpointsMessage);
        //                //var ipEndpointsMessage = $"{IpCurrent}";
        //                //var ipEndpointsMessage = messagesQueue.ToList();
        //                SendBroadCast(Encoding.UTF8.GetBytes(ipEndpointsMessage), EndPointVariable.Address);
        //            });
        //        }
        //        catch (SocketException ex)
        //        {
        //            //MessageBox.Show($"SocketException: {ex.Message}");
        //            Debug.WriteLine($"SocketException: {ex.Message}");
        //        }
        //    }
        //    catch (SocketException ex)
        //    {
        //        //MessageBox.Show($"SocketException: {ex.Message}");
        //        Debug.WriteLine($"SocketException: {ex.Message}");
        //    }
        //}

        private void ConnectNew(IPAddress heyIP)
        {

            foreach (var ipConnector in messagesQueue)
            {
                var endPointMessage = Encoding.UTF8.GetBytes($"ip:{ipConnector};");
                SendBroadCast(endPointMessage, heyIP);
            }

            if (listenerSocket.LocalEndPoint is IPEndPoint myiep)
            {
                var myEndPointBytes = Encoding.UTF8.GetBytes($"ip:{myiep.Address};");
                SendBroadCast(myEndPointBytes, heyIP);
            }

            if (!messagesQueue.Contains(heyIP))
            {
                messagesQueue.Add(heyIP);
            }
        }

        private void ConnectToServerFromUI()
        {
            try
            {
                //var ipAddress = IPTextBox.Text.ToString();
                EndPointVariable = new IPEndPoint(IPAddress.Parse(ipAddress), ServerPort);

                //if (!messagesQueue.Contains(EndPointVariable.Address))
                //{
                //    messagesQueue.Add(EndPointVariable.Address);
                //    Debug.WriteLine("The ip that is stored is: {0}", EndPointVariable.Address);
                //    Debug.WriteLine("The number of stored ip is: {0}", messagesQueue.Count);
                //    Debug.WriteLine("This ip and Port werent in the queue, so we added");
                //}

                if (!messagesQueue.Contains(IPAddress.Parse(ipAddress)))
                {
                    messagesQueue.Add(IPAddress.Parse(ipAddress));
                }

                //if(!messagesQueue.Contains(IPAddress.Parse(IpCurrent)))
                //{
                //    messagesQueue.Add(IPAddress.Parse(IpCurrent));
                //}

                try
                {
                    Dispatcher.Invoke(() =>
                    {
                        //if(messagesQueue.Count == 1)
                        //{
                        //    var endPointJan = Encoding.UTF8.GetBytes("");
                        //    SendIndividual(endPointJan, EndPointVariable);
                        //}
                        //else
                        //{

                        //foreach (var ipMessageDefault in messagesQueue)
                        //    {
                        //        var endPointJan = Encoding.UTF8.GetBytes($"{ipMessageDefault.Address}");
                        //        Debug.WriteLine(ipMessageDefault);
                        //        SendBroadCast(endPointJan, ipMessageDefault);
                        //    }
                        //}
                        //var ipEndpointsMessage = GetIpEndpointsMessage(messagesQueue);
                        //var ipEndpointsMessage = $"{IpCurrent}";
                        //var ipEndpointsMessage = messagesQueue.ToList();

                        //var ipEndpointsMessage = GetIpEndpointsMessage(messagesQueue.ToList());
                        Debug.WriteLine($"Hello {IpCurrent}");
                        SendBroadCast(Encoding.UTF8.GetBytes($"ip:{IpCurrent};"), EndPointVariable.Address);

                    });
                }
                catch (SocketException ex)
                {
                    Debug.WriteLine($"SocketException: {ex.Message}");
                }
            }
            catch (SocketException ex)
            {
                Debug.WriteLine($"SocketException: {ex.Message}");
            }
        }

        private void SendMessage(string message)
        {
            try
            {
                foreach (var ipMe in messagesQueue)
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(message);
                    SendBroadCast(buffer, ipMe);
                }
                Debug.WriteLine("Called called");
            }
            catch (SocketException ex)
            {
                Debug.WriteLine($"SocketException: {ex.Message}");
            }
        }

        private void StopListening()
        {
            isListening = false;
            try
            {
                listenerThread.Join();
            }
            catch (ThreadInterruptedException ex)
            {
                // Ignore
            }
            if (isListening == false)
            {
                Debug.WriteLine("Closed the connection to hh");
            }
        }


        public void Close() => StopListening();

        public void Dispose() => listenerSocket.Dispose();

        private void SendBroadCast(byte[] buffer, IPAddress iep)
        {
            var socketJan = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //EndPointVariable = new IPEndPoint(IPAddress.Parse(ServerIpAddress), ServerPort);
            socketJan.Connect(iep, ListenerPort);
            socketJan.Send(buffer);
            socketJan.Close();
        }

        private static IPAddress? ParseIpAndPort(byte[] buffer)
        {
            string acceptedString = Encoding.UTF8.GetString(buffer);

            if (acceptedString.StartsWith("ip:"))
            {
                var splitStrings = acceptedString.Split(':', ';');
                Debug.WriteLine($"Joined {splitStrings}");

                string ip = "";
                foreach (var str in splitStrings)
                {
                    if (str.Contains('.'))
                    {
                        ip = str;
                    }
                }

                var ipOctets = ip.Split(".");
                for (int i = 0; i < ipOctets.Length; i++)
                {
                    int octet = int.Parse(ipOctets[i]);
                    if (octet < 0 || octet > 255)
                    {
                        return null;
                    }
                }

                return IPAddress.Parse(ip);
            }

            return null;
        }
    }
}