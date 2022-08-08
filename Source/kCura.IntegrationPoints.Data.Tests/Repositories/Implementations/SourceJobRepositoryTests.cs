using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Tests.Repositories.Implementations
{
    [TestFixture, Category("Unit")]
    public class SourceJobRepositoryTests : TestBase
    {
        private IObjectTypeRepository _objectTypeRepository;
        private IFieldRepository _fieldRepository;
        private IRelativityObjectManager _objectManager;

        private SourceJobRepository _instance;

        public override void SetUp()
        {
            _objectTypeRepository = Substitute.For<IObjectTypeRepository>();
            _fieldRepository = Substitute.For<IFieldRepository>();
            _objectManager = Substitute.For<IRelativityObjectManager>();
            _instance = new SourceJobRepository(_objectTypeRepository, _fieldRepository, _objectManager);
        }

        [Test]
        public void ItShouldCreateObjectType()
        {
            var expectedResult = 140653;

            var parentArtifactTypeId = 268;

            _objectTypeRepository.CreateObjectType(SourceJobDTO.ObjectTypeGuid, Domain.Constants.SPECIAL_SOURCEJOB_FIELD_NAME, parentArtifactTypeId).Returns(expectedResult);

            // ACT
            var actualResult = _instance.CreateObjectType(parentArtifactTypeId);

            // ASSERT
            Assert.That(actualResult, Is.EqualTo(expectedResult));
            _objectTypeRepository.Received(1).CreateObjectType(SourceJobDTO.ObjectTypeGuid, Domain.Constants.SPECIAL_SOURCEJOB_FIELD_NAME, parentArtifactTypeId);
        }

        [Test]
        public void ItShouldCreate()
        {
            var expectedResult = 855108;

            var sourceJobDto = new SourceJobDTO();

            _objectManager.Create(Arg.Any<ObjectTypeRef>(), Arg.Any<RelativityObjectRef>(), Arg.Any<List<FieldRefValuePair>>()).Returns(expectedResult);

            // ACT
            var actualResult = _instance.Create(sourceJobDto);

            // ASSERT
            Assert.That(actualResult, Is.EqualTo(expectedResult));
            _objectManager.Received(1).Create(Arg.Is<ObjectTypeRef>(x => x.ArtifactID == 0 && x.Guid == SourceJobDTO.ObjectTypeGuid), Arg.Any<RelativityObjectRef>(), Arg.Any<List<FieldRefValuePair>>());
        }

        [Test]
        public void ItShouldCreateFieldOnDocument()
        {
            var expectedResult = 524116;

            int sourceJobArtifactTypeId = 362279;

            _fieldRepository.CreateMultiObjectFieldOnDocument(Domain.Constants.SPECIAL_SOURCEJOB_FIELD_NAME, sourceJobArtifactTypeId).Returns(expectedResult);

            // ACT
            var actualResult = _instance.CreateFieldOnDocument(sourceJobArtifactTypeId);

            // ASSERT
            Assert.That(actualResult, Is.EqualTo(expectedResult));
            _fieldRepository.Received(1).CreateMultiObjectFieldOnDocument(Domain.Constants.SPECIAL_SOURCEJOB_FIELD_NAME, sourceJobArtifactTypeId);
        }
    }
}