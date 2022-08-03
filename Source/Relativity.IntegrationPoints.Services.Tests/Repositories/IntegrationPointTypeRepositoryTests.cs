using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using NSubstitute;
using NUnit.Framework;
using Relativity.IntegrationPoints.Services.Repositories.Implementations;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Services.Tests.Repositories
{
    [TestFixture, Category("Unit")]
    public class IntegrationPointTypeRepositoryTests : TestBase
    {
        private IntegrationPointTypeRepository _integrationPointTypeRepository;
        private IRelativityObjectManager _objectManager;

        public override void SetUp()
        {
            _objectManager = Substitute.For<IRelativityObjectManager>();
            IRelativityObjectManagerService objectManagerService = Substitute.For<IRelativityObjectManagerService>();
            objectManagerService.RelativityObjectManager.Returns(_objectManager);

            _integrationPointTypeRepository = new IntegrationPointTypeRepository(objectManagerService);
        }

        [Test]
        public void ItShouldRetrieveAllIntegrationPointTypes()
        {
            var expectedResult = new List<IntegrationPointType>
            {
                new IntegrationPointType
                {
                    ArtifactId = 481,
                    Name = "name_871"
                },
                new IntegrationPointType
                {
                    ArtifactId = 377,
                    Name = "name_454"
                }
            };

            _objectManager.Query<IntegrationPointType>(Arg.Any<QueryRequest>()).Returns(expectedResult);

            IList<IntegrationPointTypeModel> actualResult = _integrationPointTypeRepository.GetIntegrationPointTypes();

            Assert.That(actualResult,
                Is.EquivalentTo(expectedResult).Using(new Func<IntegrationPointTypeModel, IntegrationPointType, bool>((x, y) => (x.Name == y.Name) && (x.ArtifactId == y.ArtifactId))));
        }
    }
}