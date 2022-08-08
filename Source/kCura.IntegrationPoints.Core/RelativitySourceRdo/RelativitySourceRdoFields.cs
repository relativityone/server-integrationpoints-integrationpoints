using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.Services.Interfaces.Field.Models;

namespace kCura.IntegrationPoints.Core.RelativitySourceRdo
{
    public class RelativitySourceRdoFields : IRelativitySourceRdoFields
    {
        private IFieldQueryRepository _fieldQueryRepository;
        private IArtifactGuidRepository _artifactGuidRepository;
        private IFieldRepository _fieldRepository;

        private readonly IRepositoryFactory _repositoryFactory;

        public RelativitySourceRdoFields(IRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public void CreateFields(int workspaceId, IDictionary<Guid, BaseFieldRequest> fields)
        {
            _fieldRepository = _repositoryFactory.GetFieldRepository(workspaceId);
            _fieldQueryRepository = _repositoryFactory.GetFieldQueryRepository(workspaceId);
            _artifactGuidRepository = _repositoryFactory.GetArtifactGuidRepository(workspaceId);

            foreach (Guid fieldGuid in fields.Keys)
            {
                if (!FieldWithGuidExists(fieldGuid))
                {
                    CreateField(fieldGuid, fields[fieldGuid]);
                }
            }
        }

        private bool FieldWithGuidExists(Guid guid)
        {
            return _artifactGuidRepository.GuidExists(guid);
        }

        private void CreateField(Guid guid, BaseFieldRequest field)
        {
            int descriptorArtifactTypeId = field.ObjectType.ArtifactTypeID;
            int fieldId;
            if (FieldWithoutGuidExists(field, descriptorArtifactTypeId))
            {
                fieldId = GetExistingFieldArtifactId(field, descriptorArtifactTypeId);
            }
            else
            {
                fieldId = CreateField(field);
            }
            AssignGuid(fieldId, guid);
        }

        private bool FieldWithoutGuidExists(BaseFieldRequest field, int descriptorArtifactTypeId)
        {
            return RetrieveField(field, descriptorArtifactTypeId) != null;
        }

        private int GetExistingFieldArtifactId(BaseFieldRequest field, int descriptorArtifactTypeId)
        {
            return RetrieveField(field, descriptorArtifactTypeId).ArtifactId;
        }

        private ArtifactDTO RetrieveField(BaseFieldRequest field, int descriptorArtifactTypeId)
        {
            return _fieldQueryRepository.RetrieveField(descriptorArtifactTypeId, field.Name, field.GetFieldTypeName(), new HashSet<string>
            {
                Constants.Fields.ArtifactId
            });
        }

        private int CreateField(BaseFieldRequest field)
        {
            return _fieldRepository.CreateObjectTypeField(field);
        }

        private void AssignGuid(int fieldId, Guid guid)
        {
            _artifactGuidRepository.InsertArtifactGuidForArtifactId(fieldId, guid);
        }
    }
}