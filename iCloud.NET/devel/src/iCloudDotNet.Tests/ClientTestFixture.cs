using System;
using NUnit.Framework;

namespace AppleICloudDotNet.Tests
{
    [TestFixture]
    public class ClientTestFixture
    {
        //private readonly IDictionary<string, string> config = TestConfig.Get();

        private AppleICloudClient client;

        [SetUp]
        public void FixtureSetUp()
        {
            this.client = new AppleICloudClient();
        }

        [TearDown]
        public void FixtureTearDown()
        {
        }

        [Test]
        public void InitializeTest()
        {
            Assert.DoesNotThrow(() => this.client.Initialize());
        }

        [Test]
        public void SignInTest()
        {
            var args = Environment.GetCommandLineArgs();
            Assert.DoesNotThrow(() => this.client.Initialize());

            //Assert.DoesNotThrow(() => this.client.SignIn(config["UserName"], config["Password"]));
            Assert.DoesNotThrow(() => this.client.SignIn("bhart010101@me.com", "$ME2n2ikp141"));
        }

        [Test]
        public void SignOutTest()
        {
            Assert.DoesNotThrow(() => this.client.SignOut());
        }
    }
}