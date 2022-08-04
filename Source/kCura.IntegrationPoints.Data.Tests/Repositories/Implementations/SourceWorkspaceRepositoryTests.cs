using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Tests.Repositories.Implementations
{
    [TestFixture, Category("Unit")]
    public class SourceWorkspaceRepositoryTests : TestBase
    {
        private IObjectTypeRepository _objectTypeRepository;
        private IFieldRepository _fieldRepository;
        private IRelativityObjectManager _relativityObjectManager;
        private IHelper _helper;

        private IAPILog _logApi;

        private SourceWorkspaceRepository _instance;

        public override void SetUp()
        {
            _objectTypeRepository = Substitute.For<IObjectTypeRepository>();
            _fieldRepository = Substitute.For<IFieldRepository>();
            _relativityObjectManager = Substitute.For<IRelativityObjectManager>();
            _helper = Substitute.For<IHelper>();
            _logApi = Substitute.For<IAPILog>();

            _instance = new SourceWorkspaceRepository(_helper, _objectTypeRepository, _fieldRepository, _relativityObjectManager);
        }

        [Test]
        public void ItShouldCreateObjectType()
        {
            var expectedResult = 554395;

            var parentArtifactTypeId = 275;

            _objectTypeRepository.CreateObjectType(SourceWorkspaceDTO.ObjectTypeGuid, Domain.Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME, parentArtifactTypeId).Returns(expectedResult);

            // ACT
            var actualResult = _instance.CreateObjectType(parentArtifactTypeId);

            // ASSERT
            Assert.That(actualResult, Is.EqualTo(expectedResult));
            _objectTypeRepository.Received(1).CreateObjectType(SourceWorkspaceDTO.ObjectTypeGuid, Domain.Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME, parentArtifactTypeId);
        }

        [Test]
        public void ItShouldCreate()
        {
            var expectedResult = 268763;

            var sourceWorkspaceDto = new SourceWorkspaceDTO();

            _relativityObjectManager.Create(Arg.Any<ObjectTypeRef>(), Arg.Any<List<FieldRefValuePair>>()).Returns(expectedResult);

            // ACT
            var actualResult = _instance.Create(sourceWorkspaceDto);

            // ASSERT
            Assert.That(actualResult, Is.EqualTo(expectedResult));
            _relativityObjectManager.Received(1).Create(Arg.Is<ObjectTypeRef>(x => x.ArtifactID == 0 && x.Guid == SourceWorkspaceDTO.ObjectTypeGuid), Arg.Any<List<FieldRefValuePair>>());
        }

        [Test]
        public void ItShouldUpdate()
        {
            var sourceWorkspaceDto = new SourceWorkspaceDTO
            {
                ArtifactId = 199398,
                ArtifactTypeId = 260895
            };

            // ACT
            _instance.Update(sourceWorkspaceDto);

            // ASSERT
            _relativityObjectManager.Received(1).Update(sourceWorkspaceDto.ArtifactId, Arg.Any<List<FieldRefValuePair>>());
        }

        [Test]
        public void ItShouldCreateFieldOnDocument()
        {
            var expectedResult = 990339;

            int sourceWorkspaceObjectTypeId = 214619;

            _fieldRepository.CreateMultiObjectFieldOnDocument(Domain.Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME, sourceWorkspaceObjectTypeId).Returns(expectedResult);

            // ACT
            var actualResult = _instance.CreateFieldOnDocument(sourceWorkspaceObjectTypeId);

            // ASSERT
            Assert.That(actualResult, Is.EqualTo(expectedResult));
            _fieldRepository.Received(1).CreateMultiObjectFieldOnDocument(Domain.Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME, sourceWorkspaceObjectTypeId);
        }

        [Test]
        public void ItShouldRetrieveForSourceWorkspaceId()
        {
            var rdo = new RelativityObject
            {
                ArtifactID = 348296,
                FieldValues = new List<FieldValuePair>()
            };

            var expectedResult = new SourceWorkspaceDTO
            {
                ArtifactId = rdo.ArtifactID
            };

            _relativityObjectManager.Query(Arg.Any<QueryRequest>()).Returns(new List<RelativityObject> { rdo });

            // ACT
            var actualResult = _instance.RetrieveForSourceWorkspaceId(156272, "fed_name_503", 541);

            // ASSERT
            Assert.That(actualResult.ArtifactId, Is.EqualTo(expectedResult.ArtifactId));
            _relativityObjectManager.Received(1)
                .Query(
                    Arg.Is<QueryRequest>(x =>
                        x.ObjectType.Guid == SourceWorkspaceDTO.ObjectTypeGuid
                    ));
        }

        [Test]
        public void ItShouldReturnNull_WhenObjectManager_ReturnedNull()
        {
            _relativityObjectManager.Query(Arg.Any<QueryRequest>()).Returns((List<RelativityObject>)null);

            // ACT
            var actualResult = _instance.RetrieveForSourceWorkspaceId(156272, "fed_name_503", 541);

            // ASSERT
            Assert.IsNull(actualResult);
        }

        [Test]
        public void ItShouldThrowExceptionWhenRetrieveForSourceWorkspaceId()
        {
            _relativityObjectManager.Query(Arg.Any<QueryRequest>()).Throws(new Exception());

            // ACT
            Assert.Throws<Exception>(() => _instance.RetrieveForSourceWorkspaceId(156272, "fed_name_503", 541));

            // ASSERT

            _logApi.LogError(Arg.Any<string>(), Arg.Any<string>());
        }
    }
}