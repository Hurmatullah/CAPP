﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ChatBackend
{
    public class User : IDisposable
    {
        public ConcurrentQueue<IPAddress> IPAddressesQueue;

        public string IP { get; set; } = "192.168.70.135";

        public int BufferSize { get; set; } = 1024;

        public int Port { get; set; } = 12000;

        public Socket listenerSocket;

        public User()
        {
            IPAddressesQueue = new();
        }

        public void Listen()
        {
            IPAddress ipAddress = IPAddress.Parse(IP);
            listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var listenerEndPointVariable = new IPEndPoint(ipAddress, Port);
            listenerSocket.Bind(listenerEndPointVariable);
            listenerSocket.Listen();
        }

        public void Connect(IPAddress receivedIP)
        {
            foreach (var ipConnector in IPAddressesQueue)
            {
                var endPointMessage = Encoding.UTF8.GetBytes($"Join:{ipConnector};");
                SendBroadCast(endPointMessage, receivedIP);
            }

            if (listenerSocket.LocalEndPoint is IPEndPoint myiep)
            {
                var myEndPointBytes = Encoding.UTF8.GetBytes($"Join:{myiep.Address};");
                SendBroadCast(myEndPointBytes, receivedIP);
            }

            if (!IPAddressesQueue.Contains(receivedIP))
            {
                IPAddressesQueue.Enqueue(receivedIP);
            }
        }

        public void SendMessage(byte[] buffer)
        {
            foreach (var ipAddresses in IPAddressesQueue)
            {
                SendBroadCast(buffer, ipAddresses);
            }
        }

        public void SendBroadCast(byte[] buffer, IPAddress ipAdd)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ipAdd, Port);
            socket.Send(buffer);
            socket.Close();
        }

        public void Close()
        {
            listenerSocket.Close();
        }

        public void Dispose()
        {
            listenerSocket.Dispose();
        }

        public IPAddress? ValidateIp(byte[] buffer)
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

        public IPAddress? ValidateDisconnectionOfIp(byte[] buffer)
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
