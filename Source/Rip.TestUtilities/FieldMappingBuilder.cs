using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using Relativity;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace Rip.TestUtilities
{
    public class FieldMappingBuilder
    {
        private readonly IRepositoryFactory _repositoryFactory;

        private IList<FieldEntry> _sourceFields;
        private IList<FieldEntry> _destinationFields;
        private IFieldQueryRepository _sourceWorkspaceFieldQueryRepository;
        private IFieldQueryRepository _destinationWorkspaceFieldQueryRepository;

        public FieldMappingBuilder(IRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public FieldMappingBuilder WithSourceWorkspaceID(int sourceWorkspaceID)
        {
            _sourceWorkspaceFieldQueryRepository = CreateFieldQueryRepository(sourceWorkspaceID);
            return this;
        }

        public FieldMappingBuilder WithDestinationWorkspaceID(int destinationWorkspaceID)
        {
            _destinationWorkspaceFieldQueryRepository = CreateFieldQueryRepository(destinationWorkspaceID);
            return this;
        }

        public FieldMappingBuilder WithSourceFields(IList<FieldEntry> sourceFields)
        {
            _sourceFields = sourceFields;
            return this;
        }

        public FieldMappingBuilder WithDestinationFields(IList<FieldEntry> destinationFields)
        {
            _destinationFields = destinationFields;
            return this;
        }

        public FieldMap[] Build()
        {
            int count = _sourceFields.Count;
            var fieldMapping = new List<FieldMap>(count + 1);
            var identifierFieldMap = CreateIdentifierFieldMap();
            fieldMapping.Add(identifierFieldMap);

            for (int i = 0; i < count; i++)
            {
                var fieldMap = new FieldMap
                {
                    SourceField = _sourceFields[i],
                    DestinationField = _destinationFields[i],
                    FieldMapType = FieldMapTypeEnum.None
                };
                fieldMapping.Add(fieldMap);
            }

            return fieldMapping.ToArray();
        }

        private FieldMap CreateIdentifierFieldMap()
        {
            var sourceDto = RetrieveIdentifierField(_sourceWorkspaceFieldQueryRepository);
            var destinationDto = RetrieveIdentifierField(_destinationWorkspaceFieldQueryRepository);

            var fieldMap = new FieldMap()
            {
                SourceField = CreateIdentifierFieldEntry(sourceDto),
                DestinationField = CreateIdentifierFieldEntry(destinationDto),
                FieldMapType = FieldMapTypeEnum.Identifier
            };

            return fieldMap;
        }

        private ArtifactDTO RetrieveIdentifierField(IFieldQueryRepository fieldQueryRepository)
        {
            return fieldQueryRepository.RetrieveIdentifierField((int) ArtifactType.Document);
        }

        private static FieldEntry CreateIdentifierFieldEntry(ArtifactDTO fieldDto)
        {
            return new FieldEntry
            {
                FieldIdentifier = fieldDto.ArtifactId.ToString(),
                DisplayName = $"{fieldDto.Fields.First(field => field.Name == "Name")} [Object Identifier]",
                IsIdentifier = true
            };
        }

        private IFieldQueryRepository CreateFieldQueryRepository(int workspaceID)
        {
            IFieldQueryRepository fieldQueryRepository = _repositoryFactory.GetFieldQueryRepository(workspaceID);
            return fieldQueryRepository;
        }
    }
}
