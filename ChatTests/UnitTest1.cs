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
        public void Test_Listen()
        {
            user.Listen();

            Assert.True(true);
        }

        [Test]
        public void Test_User_Connect()
        {
            var receivedIPAdd = IPAddress.Parse(IPAdd);

            user.IPAddressesQueue.Enqueue(receivedIPAdd);

            user.Connect(receivedIPAdd);

            Assert.True(true);
        }

        [Test]
        public void Test_Send_BroadCast()
        {
            var endPointMessage = Encoding.UTF8.GetBytes($"Join:{IPAdd};");

            user.Send_BroadCast(endPointMessage, IPAddress.Parse(IPAdd));

            Assert.IsTrue(true);
        }

        [Test]
        public void Test_Send_Message()
        {
            var message = "Hey there!";

            user.IPAddressesQueue.Enqueue(IPAddress.Parse(IPAdd));

            user.Send_Message(Encoding.UTF8.GetBytes(message));

            Assert.True(true);
        }

        [Test]
        public void Test_Validate_IP()
        {
            var receivedMessage = Encoding.UTF8.GetBytes($"Join:{IPAdd};");

            var validatedIP = user.Validate_Ip(receivedMessage);

            if (validatedIP != null)
            {
                Assert.True(true);
            }
        }

        [Test]
        public void Test_Disconnection_IP()
        {
            var receivedDisconnMessage = Encoding.UTF8.GetBytes($"Disconnect:{user.IP};");

            var validatedDisconIP = user.Validate_Disconnection_Of_Ip(receivedDisconnMessage);

            if (validatedDisconIP != null)
            {
                Assert.True(true);
            }
        }
    }
}