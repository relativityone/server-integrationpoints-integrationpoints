using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.RelativitySourceRdo;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.RelativitySourceRdo
{
    [TestFixture, Category("Unit")]
    public class RelativitySourceRdoObjectTypeTests : TestBase
    {
        private const int _WORKSPACE_ID = 326544;
        private IRelativityProviderObjectRepository _relativityObjectRepository;
        private IObjectTypeRepository _objectTypeRepository;
        private IArtifactGuidRepository _artifactGuidRepository;
        private ITabRepository _tabRepository;
        private RelativitySourceRdoObjectType _instance;

        public override void SetUp()
        {
            _relativityObjectRepository = Substitute.For<IRelativityProviderObjectRepository>();
            _objectTypeRepository = Substitute.For<IObjectTypeRepository>();
            _artifactGuidRepository = Substitute.For<IArtifactGuidRepository>();
            _tabRepository = Substitute.For<ITabRepository>();

            var repositoryFactory = Substitute.For<IRepositoryFactory>();
            repositoryFactory.GetDestinationObjectTypeRepository(_WORKSPACE_ID).Returns(_objectTypeRepository);
            repositoryFactory.GetArtifactGuidRepository(_WORKSPACE_ID).Returns(_artifactGuidRepository);
            repositoryFactory.GetTabRepository(_WORKSPACE_ID).Returns(_tabRepository);

            _instance = new RelativitySourceRdoObjectType(_relativityObjectRepository, repositoryFactory);
        }

        [Test]
        public void ItShouldCreateObjectTypeWhenItDoesNotExist()
        {
            Guid objectTypeGuid = Guid.NewGuid();
            string objectTypeName = "object_type_name_817";
            int parentArtifactTypeId = 945727;

            int objectTypeArtifactId = 937543;
            int tabId = 772992;

            int expectedDescriptorArtifactTypeId = 636492;

            _objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(objectTypeGuid).Returns(x => { throw new TypeLoadException(); }, x => expectedDescriptorArtifactTypeId);
            _objectTypeRepository.RetrieveObjectTypeArtifactId(objectTypeName).Returns((int?)null);
            _relativityObjectRepository.CreateObjectType(parentArtifactTypeId).Returns(objectTypeArtifactId);
            _tabRepository.RetrieveTabArtifactId(expectedDescriptorArtifactTypeId, objectTypeName).Returns(tabId);

            // ACT
            var actualResult = _instance.CreateObjectType(_WORKSPACE_ID, objectTypeGuid, objectTypeName, parentArtifactTypeId);

            // ASSERT
            Assert.That(actualResult, Is.EqualTo(expectedDescriptorArtifactTypeId));

            _objectTypeRepository.Received(2).RetrieveObjectTypeDescriptorArtifactTypeId(objectTypeGuid);
            _objectTypeRepository.Received(1).RetrieveObjectTypeArtifactId(objectTypeName);
            _relativityObjectRepository.Received(1).CreateObjectType(parentArtifactTypeId);
            _artifactGuidRepository.Received(1).InsertArtifactGuidForArtifactId(objectTypeArtifactId, objectTypeGuid);
            _tabRepository.Received(1).RetrieveTabArtifactId(expectedDescriptorArtifactTypeId, objectTypeName);
            _tabRepository.Received(1).Delete(tabId);
        }

        [Test]
        public void ItShouldUpdateObjectTypeWhenItExistsWithoutGuid()
        {
            Guid objectTypeGuid = Guid.NewGuid();
            string objectTypeName = "object_type_name_694";
            int parentArtifactTypeId = 781336;

            int objectTypeArtifactId = 206691;
            int tabId = 519405;

            int expectedDescriptorArtifactTypeId = 170521;

            _objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(objectTypeGuid).Returns(x => { throw new TypeLoadException(); }, x => expectedDescriptorArtifactTypeId);
            _objectTypeRepository.RetrieveObjectTypeArtifactId(objectTypeName).Returns(objectTypeArtifactId);
            _tabRepository.RetrieveTabArtifactId(expectedDescriptorArtifactTypeId, objectTypeName).Returns(tabId);

            // ACT
            var actualResult = _instance.CreateObjectType(_WORKSPACE_ID, objectTypeGuid, objectTypeName, parentArtifactTypeId);

            // ASSERT
            Assert.That(actualResult, Is.EqualTo(expectedDescriptorArtifactTypeId));

            _objectTypeRepository.Received(2).RetrieveObjectTypeDescriptorArtifactTypeId(objectTypeGuid);
            _objectTypeRepository.Received(2).RetrieveObjectTypeArtifactId(objectTypeName);
            _artifactGuidRepository.Received(1).InsertArtifactGuidForArtifactId(objectTypeArtifactId, objectTypeGuid);
            _tabRepository.Received(1).RetrieveTabArtifactId(expectedDescriptorArtifactTypeId, objectTypeName);
            _tabRepository.Received(1).Delete(tabId);

            _relativityObjectRepository.DidNotReceive().CreateObjectType(parentArtifactTypeId);
        }

        [Test]
        public void ItShouldReturnDescriptorArtifactTypeIdForExistingObjectType()
        {
            Guid objectTypeGuid = Guid.NewGuid();

            int expectedDescriptorArtifactTypeId = 509502;

            _objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(objectTypeGuid).Returns(expectedDescriptorArtifactTypeId);

            // ACT
            var actualResult = _instance.CreateObjectType(_WORKSPACE_ID, objectTypeGuid, "object_type_name_602", 492560);

            // ASSERT
            Assert.That(actualResult, Is.EqualTo(expectedDescriptorArtifactTypeId));

            _objectTypeRepository.Received(2).RetrieveObjectTypeDescriptorArtifactTypeId(objectTypeGuid);

            _objectTypeRepository.DidNotReceive().RetrieveObjectTypeArtifactId(Arg.Any<string>());
            _artifactGuidRepository.DidNotReceive().InsertArtifactGuidForArtifactId(Arg.Any<int>(), Arg.Any<Guid>());
            _tabRepository.DidNotReceive().RetrieveTabArtifactId(Arg.Any<int>(), Arg.Any<string>());
            _tabRepository.DidNotReceive().Delete(Arg.Any<int>());
            _relativityObjectRepository.DidNotReceive().CreateObjectType(Arg.Any<int>());
        }
    }
}
