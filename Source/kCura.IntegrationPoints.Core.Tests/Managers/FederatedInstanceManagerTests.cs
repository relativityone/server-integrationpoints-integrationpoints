using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Domain.Models;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Managers
{
    [TestFixture, Category("Unit")]
    public class FederatedInstanceManagerTests
    {
        [Test]
        public void TestLocalInstance()
        {
            var testInstance = new FederatedInstanceManager();

            var federatedInstance = testInstance.RetrieveFederatedInstanceByArtifactId(null);

            Assert.That(federatedInstance, Is.EqualTo(FederatedInstanceManager.LocalInstance));
        }

        [Test]
        public void TestRetrieveFederatedInstance()
        {
            // arrange
            var federatedInstanceArtifactId = 1024;
            var testInstance = new FederatedInstanceManager();

            // act
            var federatedInstance = testInstance.RetrieveFederatedInstanceByArtifactId(federatedInstanceArtifactId);

            // assert
            Assert.That(federatedInstance, Is.Null);
        }

        [Test]
        public void TestRetrieveAllHasLocalInstance()
        {
            // arrange
            var testInstance = new FederatedInstanceManager();

            // act
            List<FederatedInstanceDto> federatedInstances = testInstance.RetrieveAll().ToList();

            Assert.That(federatedInstances, Is.Not.Null);
            Assert.That(federatedInstances.Count(), Is.EqualTo(1));
            Assert.That(federatedInstances[0].Name, Is.EqualTo("This Instance"));
            Assert.That(federatedInstances[0].ArtifactId, Is.EqualTo(null));
        }
    }
}
