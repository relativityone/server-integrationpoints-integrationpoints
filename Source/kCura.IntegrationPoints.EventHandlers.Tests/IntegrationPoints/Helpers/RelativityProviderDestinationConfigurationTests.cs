using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Contracts;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Tests.IntegrationPoints.Helpers
{
    [TestFixture, Category("Unit")]
    public class RelativityProviderDestinationConfigurationTests
    {
        private RelativityProviderDestinationConfiguration _instace;
        private IEHHelper _helper;
        private IFederatedInstanceManager _federatedInstanceManager;
        private IObjectTypeRepository _objectTypeRepository;
        private const int _ARTIFACT_TYPE_ID = 0;
        private const int _FEDERATED_INSTANCE_ID = 3;
        private const string _ARTIFACT_TYPE_NAME = "ArtifactTypeName";
        private const string _DESTINATION_RELATIVITY_INSTANCE = "DestinationRelativityInstance";

        [SetUp]
        public void SetUp()
        {
            _helper = Substitute.For<IEHHelper>();
            _federatedInstanceManager = Substitute.For<IFederatedInstanceManager>();
            _objectTypeRepository = Substitute.For<IObjectTypeRepository>();

            var repositoryFactory = Substitute.For<IRepositoryFactory>();
            repositoryFactory.GetObjectTypeRepository(Arg.Any<int>()).Returns(_objectTypeRepository);

            _instace = new RelativityProviderDestinationConfiguration(_helper, _federatedInstanceManager, repositoryFactory);
        }

        [TestCase("Other Instance")]
        public void ItShouldUpdateNames(string instanceName)
        {
            // arrange
            var settings = GetSettings();
            MockFederatedInstanceManager(_FEDERATED_INSTANCE_ID, instanceName);

            // act
            _instace.UpdateNames(settings, new EventHandler.Artifact(934580, 990562, 533988, "", false, null));

            //assert
            Assert.AreEqual("RDO", settings[_ARTIFACT_TYPE_NAME]);
            Assert.AreEqual(instanceName, settings[_DESTINATION_RELATIVITY_INSTANCE]);
        }
    
        private void MockFederatedInstanceManager(int instanceId,string federatedInstanceName)
        {
            var federatedInstanceDto = Substitute.For<FederatedInstanceDto>();
            federatedInstanceDto.Name = federatedInstanceName;
            _federatedInstanceManager.RetrieveFederatedInstanceByArtifactId(instanceId).Returns(federatedInstanceDto);
        }

        private IDictionary<string, object> GetSettings()
        {
            var settings = new Dictionary<string, object>
            {
                {nameof(DestinationConfiguration.ArtifactTypeId), _ARTIFACT_TYPE_ID},
                {nameof(DestinationConfiguration.FederatedInstanceArtifactId), _FEDERATED_INSTANCE_ID}
            };

            return settings;
        }
    }
}
