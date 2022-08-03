using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.RelativitySourceRdo;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;
using Relativity;

namespace kCura.IntegrationPoints.Core.Tests.RelativitySourceRdo
{
    [TestFixture, Category("Unit")]
    public class RelativitySourceRdoDocumentFieldTests : TestBase
    {
        private const int _WORKSPACE_ID = 231293;
        private RelativitySourceRdoDocumentField _instance;
        private IRelativityProviderObjectRepository _relativityProviderObjectRepository;
        private IArtifactGuidRepository _artifactGuidRepository;
        private IFieldQueryRepository _fieldQueryRepository;
        private IFieldRepository _fieldRepository;

        public override void SetUp()
        {
            _relativityProviderObjectRepository = Substitute.For<IRelativityProviderObjectRepository>();

            _artifactGuidRepository = Substitute.For<IArtifactGuidRepository>();
            _fieldQueryRepository = Substitute.For<IFieldQueryRepository>();
            _fieldRepository = Substitute.For<IFieldRepository>();

            IRepositoryFactory repositoryFactory = Substitute.For<IRepositoryFactory>();
            repositoryFactory.GetFieldRepository(_WORKSPACE_ID).Returns(_fieldRepository);
            repositoryFactory.GetFieldQueryRepository(_WORKSPACE_ID).Returns(_fieldQueryRepository);
            repositoryFactory.GetArtifactGuidRepository(_WORKSPACE_ID).Returns(_artifactGuidRepository);

            _instance = new RelativitySourceRdoDocumentField(_relativityProviderObjectRepository, repositoryFactory);
        }

        [Test]
        public void ItShouldCreateDocumentFieldWhenItDoesNotExist()
        {
            Guid documentFieldGuid = Guid.NewGuid();
            string fieldName = "field_name_894";
            int objectTypeDescriptorArtifactTypeId = 295167;

            int artifactId = 267180;
            int artifactViewFieldId = 819157;

            _artifactGuidRepository.GuidExists(documentFieldGuid).Returns(false);
            _fieldQueryRepository.RetrieveField((int) ArtifactType.Document, fieldName, FieldTypes.MultipleObject, Arg.Is<HashSet<string>>(x => x.Contains(Constants.Fields.ArtifactId)))
                .Returns((ArtifactDTO) null);
            _relativityProviderObjectRepository.CreateFieldOnDocument(objectTypeDescriptorArtifactTypeId).Returns(artifactId);
            _fieldQueryRepository.RetrieveArtifactViewFieldId(artifactId).Returns(artifactViewFieldId);

            // ACT
            _instance.CreateDocumentField(_WORKSPACE_ID, documentFieldGuid, fieldName, objectTypeDescriptorArtifactTypeId);

            // ASSERT
            _artifactGuidRepository.Received(1).GuidExists(documentFieldGuid);
            _fieldQueryRepository.Received(1)
                .RetrieveField((int) ArtifactType.Document, fieldName, FieldTypes.MultipleObject, Arg.Is<HashSet<string>>(x => x.Contains(Constants.Fields.ArtifactId)));
            _relativityProviderObjectRepository.Received(1).CreateFieldOnDocument(objectTypeDescriptorArtifactTypeId);
            _artifactGuidRepository.Received(1).InsertArtifactGuidForArtifactId(artifactId, documentFieldGuid);
            _fieldQueryRepository.Received(1).RetrieveArtifactViewFieldId(artifactId);
            _fieldRepository.Received(1).UpdateFilterType(artifactViewFieldId, DocumentFieldsConstants.POPUP_FILTER_TYPE_NAME);
            _fieldRepository.Received(1).SetOverlayBehavior(artifactId, true);
        }

        [Test]
        public void ItShouldUpdateDocumentFieldWhenItExistsWithoutGuid()
        {
            Guid documentFieldGuid = Guid.NewGuid();
            string fieldName = "field_name_607";
            int objectTypeDescriptorArtifactTypeId = 571447;

            int artifactId = 392512;
            int artifactViewFieldId = 556810;

            _artifactGuidRepository.GuidExists(documentFieldGuid).Returns(false);
            _fieldQueryRepository.RetrieveField((int) ArtifactType.Document, fieldName, FieldTypes.MultipleObject, Arg.Is<HashSet<string>>(x => x.Contains(Constants.Fields.ArtifactId)))
                .Returns(new ArtifactDTO(artifactId, 1, "", new List<ArtifactFieldDTO>()));
            _fieldQueryRepository.RetrieveArtifactViewFieldId(artifactId).Returns(artifactViewFieldId);

            // ACT
            _instance.CreateDocumentField(_WORKSPACE_ID, documentFieldGuid, fieldName, objectTypeDescriptorArtifactTypeId);

            // ASSERT
            _artifactGuidRepository.Received(1).GuidExists(documentFieldGuid);
            _fieldQueryRepository.Received(2)
                .RetrieveField((int) ArtifactType.Document, fieldName, FieldTypes.MultipleObject, Arg.Is<HashSet<string>>(x => x.Contains(Constants.Fields.ArtifactId)));
            _artifactGuidRepository.Received(1).InsertArtifactGuidForArtifactId(artifactId, documentFieldGuid);
            _fieldQueryRepository.Received(1).RetrieveArtifactViewFieldId(artifactId);
            _fieldRepository.Received(1).UpdateFilterType(artifactViewFieldId, DocumentFieldsConstants.POPUP_FILTER_TYPE_NAME);
            _fieldRepository.Received(1).SetOverlayBehavior(artifactId, true);

            _relativityProviderObjectRepository.DidNotReceive().CreateFieldOnDocument(objectTypeDescriptorArtifactTypeId);
        }

        [Test]
        public void ItShouldSkipCreatingWhenFieldAlreadyExists()
        {
            Guid documentFieldGuid = Guid.NewGuid();

            _artifactGuidRepository.GuidExists(documentFieldGuid).Returns(true);

            // ACT
            _instance.CreateDocumentField(_WORKSPACE_ID, documentFieldGuid, "field_name_113", 143798);

            // ASSERT
            _artifactGuidRepository.Received(1).GuidExists(documentFieldGuid);

            _fieldQueryRepository.DidNotReceive().RetrieveField(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<HashSet<string>>());
            _relativityProviderObjectRepository.DidNotReceive().CreateFieldOnDocument(Arg.Any<int>());
            _artifactGuidRepository.DidNotReceive().InsertArtifactGuidForArtifactId(Arg.Any<int>(), Arg.Any<Guid>());
            _fieldQueryRepository.DidNotReceive().RetrieveArtifactViewFieldId(Arg.Any<int>());
            _fieldRepository.DidNotReceive().UpdateFilterType(Arg.Any<int>(), Arg.Any<string>());
            _fieldRepository.DidNotReceive().SetOverlayBehavior(Arg.Any<int>(), Arg.Any<bool>());
        }
    }
}