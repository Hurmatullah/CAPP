using ChatBackend;
using System.Net;
using System.Text;

namespace ChatTests
{
    public class Tests
    {
        private User user;

        private string IPAdd = "192.168.75.140";

        public Tests()
        {
            user = new User();
        }

        [Test]
        public void TestListen()
        {
            user.Listen();
        }

        [Test]
        public void TestUserConnect()
        {
            var receivedIPAdd = IPAddress.Parse(IPAdd);
            user.ipAddressesQueue.Enqueue(receivedIPAdd);
            user.Connect(receivedIPAdd);
        }

        [Test]
        public void TestSendBroadCast()
        {
            var endPointMessage = Encoding.UTF8.GetBytes($"Join:{IPAdd};");
            user.SendBroadCast(endPointMessage, IPAddress.Parse(IPAdd));
        }

        [Test]
        public void TestSendMessage()
        {
            var message = "Hey there!";
            user.ipAddressesQueue.Enqueue(IPAddress.Parse(IPAdd));
            user.SendMessage(Encoding.UTF8.GetBytes(message));
        }

        [Test]
        public void TestValidateIP()
        {
            var receivedMessage = Encoding.UTF8.GetBytes($"Join:{IPAdd};");
            user.ValidateIp(receivedMessage);
        }

        [Test]
        public void TestDisconnectionIP()
        {
            var receivedDisconnMessage = Encoding.UTF8.GetBytes($"Disconnect:{user.IP};");
            user.ValidateIpDisconnection(receivedDisconnMessage);
        }
    }
}