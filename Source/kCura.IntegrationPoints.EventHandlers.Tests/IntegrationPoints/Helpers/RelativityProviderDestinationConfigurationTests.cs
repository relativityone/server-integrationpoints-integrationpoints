using System.Collections.Generic;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using kCura.IntegrationPoints.Synchronizers.RDO;
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
        private IObjectTypeRepository _objectTypeRepository;
        private const int _ARTIFACT_TYPE_ID = 0;
        private const string _ARTIFACT_TYPE_NAME = "ArtifactTypeName";

        [SetUp]
        public void SetUp()
        {
            _helper = Substitute.For<IEHHelper>();
            _objectTypeRepository = Substitute.For<IObjectTypeRepository>();

            var repositoryFactory = Substitute.For<IRepositoryFactory>();
            repositoryFactory.GetObjectTypeRepository(Arg.Any<int>()).Returns(_objectTypeRepository);

            _instace = new RelativityProviderDestinationConfiguration(_helper, repositoryFactory);
        }

        [Test]
        public void ItShouldUpdateNames()
        {
            // arrange
            var settings = GetSettings();

            // act
            _instace.UpdateNames(settings, new EventHandler.Artifact(934580, 990562, 533988, "", false, null));

            // assert
            Assert.AreEqual("RDO", settings[_ARTIFACT_TYPE_NAME]);
        }

        private IDictionary<string, object> GetSettings()
        {
            return new Dictionary<string, object>
            {
                { nameof(ImportSettings.ArtifactTypeId), _ARTIFACT_TYPE_ID },
            };
        }
    }
}
