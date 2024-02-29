using System.Threading.Tasks;
using NUnit.Framework;
using Relativity.Sync.Executors;

namespace Relativity.Sync.Tests.Unit.Executors
{
    [TestFixture]
    public sealed class FederatedInstanceTests
    {
        private FederatedInstance _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new FederatedInstance();
        }

        [Test]
        public async Task ItShouldReturnHardcodedInstanceId()
        {
            const int expectedFederatedInstanceId = -1;

            // act
            int federatedInstanceId = await _sut.GetInstanceIdAsync().ConfigureAwait(false);

            // assert
            Assert.AreEqual(expectedFederatedInstanceId, federatedInstanceId);
        }

        [Test]
        public async Task ItShouldReturnHardcodedInstanceName()
        {
            const string expectedFederatedInstanceName = "This Instance";

            // act
            string federatedInstanceName = await _sut.GetInstanceNameAsync().ConfigureAwait(false);

            // assert
            Assert.AreEqual(expectedFederatedInstanceName, federatedInstanceName);
        }
    }
}
