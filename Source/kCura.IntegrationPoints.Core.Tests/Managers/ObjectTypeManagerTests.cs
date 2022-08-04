using System;
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
    public class ObjectTypeManagerTests : TestBase
    {
        private IObjectTypeManager _testInstance;
        private IRepositoryFactory _repositoryFactory;
        private IObjectTypeRepository _objectTypeRepository;

        private const int _WORKSPACE_ID = 100532;

        [SetUp]
        public override void SetUp()
        {
            _repositoryFactory = Substitute.For<IRepositoryFactory>();
            _objectTypeRepository = Substitute.For<IObjectTypeRepository>();
            _testInstance = new ObjectTypeManager(_repositoryFactory);

            _repositoryFactory.GetObjectTypeRepository(_WORKSPACE_ID).Returns(_objectTypeRepository);
        }

        [Test]
        public void RetrieveObjectTypeDescriptorArtifactTypeId_GoldFlow()
        {
            // ARRANGE
            Guid objectTypeGuid = Guid.NewGuid();
            int expectedObjectTypeDescriptorArtifactTypeId = 2342423;
            _objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(objectTypeGuid)
                .Returns(expectedObjectTypeDescriptorArtifactTypeId);

            // ACT
            int result = _testInstance.RetrieveObjectTypeDescriptorArtifactTypeId(_WORKSPACE_ID, objectTypeGuid);

            // ASSERT
            Assert.AreEqual(expectedObjectTypeDescriptorArtifactTypeId, result);
        }

    }
}
