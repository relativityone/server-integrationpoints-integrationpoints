using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Managers
{
    [TestFixture, Category("Unit")]
    public class ArtifactGuidManagerTests : TestBase
    {
        private IArtifactGuidManager _testInstance;
        private IRepositoryFactory _repositoryFactory;
        private IArtifactGuidRepository _artifactGuidRepository;

        private const int _WORKSPACE_ID = 100532;

        [SetUp]
        public override void SetUp()
        {
            _repositoryFactory = Substitute.For<IRepositoryFactory>();
            _artifactGuidRepository = Substitute.For<IArtifactGuidRepository>();
            _testInstance = new ArtifactGuidManager(_repositoryFactory);

            _repositoryFactory.GetArtifactGuidRepository(_WORKSPACE_ID).Returns(_artifactGuidRepository);
        }

        [Test]
        public void GetGuidsForArtifactIds_GoldFlow()
        {
            // ARRANGE
            var artifactIds = new int[] {123456, 988767};
            var expectedResult = new Dictionary<int, Guid>()
            {
                {artifactIds[0], Guid.NewGuid() },
                {artifactIds[1], Guid.NewGuid() }
            };

            _artifactGuidRepository.GetGuidsForArtifactIds(artifactIds).Returns(expectedResult);

            // ACT
            Dictionary<int, Guid> result = _testInstance.GetGuidsForArtifactIds(_WORKSPACE_ID, artifactIds);

            // ASSERT
            Assert.AreEqual(result, expectedResult);
        }

        [Test]
        public void GetArtifactIdsForGuids_GoldFlow()
        {
            // ARRANGE
            var guids = new Guid[] { Guid.NewGuid(), Guid.NewGuid() };
            var expectedResult = new Dictionary<Guid, int>()
            {
                {guids[0], 123456 },
                {guids[1], 988767 }
            };

            _artifactGuidRepository.GetArtifactIdsForGuids(guids).Returns(expectedResult);

            // ACT
            Dictionary<Guid, int> result = _testInstance.GetArtifactIdsForGuids(_WORKSPACE_ID, guids);

            // ASSERT
            Assert.AreEqual(result, expectedResult);
        }
    }
}
