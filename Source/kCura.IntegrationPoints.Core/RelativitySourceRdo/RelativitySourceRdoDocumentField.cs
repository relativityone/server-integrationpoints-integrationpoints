using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using Relativity;

namespace kCura.IntegrationPoints.Core.RelativitySourceRdo
{
    public class RelativitySourceRdoDocumentField : IRelativitySourceRdoDocumentField
    {
        private readonly IRelativityProviderObjectRepository _relativityObjectRepository;
        private readonly IRepositoryFactory _repositoryFactory;
        private IArtifactGuidRepository _artifactGuidRepository;
        private IFieldQueryRepository _fieldQueryRepository;
        private IFieldRepository _fieldRepository;

        public RelativitySourceRdoDocumentField(IRelativityProviderObjectRepository relativityObjectRepository, IRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
            _relativityObjectRepository = relativityObjectRepository;
        }

        public void CreateDocumentField(int workspaceArtifactId, Guid documentFieldGuid, string fieldName, int objectTypeDescriptorArtifactTypeId)
        {
            _artifactGuidRepository = _repositoryFactory.GetArtifactGuidRepository(workspaceArtifactId);
            _fieldQueryRepository = _repositoryFactory.GetFieldQueryRepository(workspaceArtifactId);
            _fieldRepository = _repositoryFactory.GetFieldRepository(workspaceArtifactId);

            if (DocumentFieldWithGuidExists(documentFieldGuid))
            {
                return;
            }

            int fieldArtifactId;
            if (DocumentFieldWithoutGuidExists(fieldName))
            {
                fieldArtifactId = GetFieldArtifactId(fieldName);
            }
            else
            {
                fieldArtifactId = CreateFieldOnDocument(objectTypeDescriptorArtifactTypeId);
            }
            AssignGuid(fieldArtifactId, documentFieldGuid);

            UpdateFieldFilterType(fieldArtifactId);
            SetOverlayBehavior(fieldArtifactId);
        }

        private bool DocumentFieldWithGuidExists(Guid documentFieldGuid)
        {
            return _artifactGuidRepository.GuidExists(documentFieldGuid);
        }

        private bool DocumentFieldWithoutGuidExists(string fieldName)
        {
            return RetrieveField(fieldName) != null;
        }

        private int GetFieldArtifactId(string fieldName)
        {
            return RetrieveField(fieldName).ArtifactId;
        }

        private ArtifactDTO RetrieveField(string fieldName)
        {
            return _fieldQueryRepository.RetrieveField((int) ArtifactType.Document, fieldName, FieldTypes.MultipleObject, new HashSet<string> {Constants.Fields.ArtifactId});
        }

        private int CreateFieldOnDocument(int objectTypeDescriptorArtifactTypeId)
        {
            return _relativityObjectRepository.CreateFieldOnDocument(objectTypeDescriptorArtifactTypeId);
        }

        private void AssignGuid(int fieldArtifactId, Guid documentFieldGuid)
        {
            _artifactGuidRepository.InsertArtifactGuidForArtifactId(fieldArtifactId, documentFieldGuid);
        }

        private void UpdateFieldFilterType(int fieldArtifactId)
        {
            int? retrieveArtifactViewFieldId = _fieldQueryRepository.RetrieveArtifactViewFieldId(fieldArtifactId);
            if (!retrieveArtifactViewFieldId.HasValue)
            {
                throw new Exception("ArtifactViewFieldId not found.");
            }

            _fieldRepository.UpdateFilterType(retrieveArtifactViewFieldId.Value, DocumentFieldsConstants.POPUP_FILTER_TYPE_NAME);
        }


        private void SetOverlayBehavior(int fieldArtifactId)
        {
            _fieldRepository.SetOverlayBehavior(fieldArtifactId, true);
        }
    }
}