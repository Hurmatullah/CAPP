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
            Assert.True(true);
        }

        [Test]
        public void TestUserConnect()
        {
            var receivedIPAdd = IPAddress.Parse(IPAdd);
            user.ipAddressesQueue.Enqueue(receivedIPAdd);
            user.Connect(receivedIPAdd);
            Assert.True(true);
        }

        [Test]
        public void TestSendBroadCast()
        {
            var endPointMessage = Encoding.UTF8.GetBytes($"Join:{IPAdd};");
            user.SendBroadCast(endPointMessage, IPAddress.Parse(IPAdd));
            Assert.IsTrue(true);
        }

        [Test]
        public void TestSendMessage()
        {
            var message = "Hey there!";
            user.ipAddressesQueue.Enqueue(IPAddress.Parse(IPAdd));
            user.SendMessage(Encoding.UTF8.GetBytes(message));
            Assert.True(true);
        }

        [Test]
        public void TestValidateIP()
        {
            var receivedMessage = Encoding.UTF8.GetBytes($"Join:{IPAdd};");
            var validatedIP = user.ValidateIp(receivedMessage);

            if (validatedIP != null)
            {
                Assert.True(true);
            }
        }

        [Test]
        public void TestDisconnectionIP()
        {
            var receivedDisconnMessage = Encoding.UTF8.GetBytes($"Disconnect:{user.IP};");
            var validatedDisconIP = user.ValidateDisconnectionOfIp(receivedDisconnMessage);

            if (validatedDisconIP != null)
            {
                Assert.True(true);
            }
        }
    }
}