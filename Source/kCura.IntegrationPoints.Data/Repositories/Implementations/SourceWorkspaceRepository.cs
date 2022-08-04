using System;
using System.Linq;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
    public class SourceWorkspaceRepository : ISourceWorkspaceRepository
    {
        private readonly IObjectTypeRepository _objectTypeRepository;
        private readonly IFieldRepository _fieldRepository;
        private readonly IRelativityObjectManager _objectManager;
        private readonly IAPILog _logger;

        public SourceWorkspaceRepository(IHelper helper, IObjectTypeRepository objectTypeRepository, IFieldRepository fieldRepository, IRelativityObjectManager objectManager)
        {
            _objectTypeRepository = objectTypeRepository;
            _fieldRepository = fieldRepository;
            _objectManager = objectManager;

            _logger = helper.GetLoggerFactory().GetLogger().ForContext<SourceWorkspaceRepository>();
        }

        public int CreateObjectType(int parentArtifactTypeId)
        {
            try
            {
                return _objectTypeRepository.CreateObjectType(SourceWorkspaceDTO.ObjectTypeGuid, Domain.Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME, parentArtifactTypeId);
            }
            catch (Exception e)
            {
                throw new Exception(RelativityProvider.ERROR_CREATE_SOURCE_CASE_FIELDS_ON_DESTINATION_CASE, e);
            }
        }

        public int CreateFieldOnDocument(int sourceWorkspaceObjectTypeId)
        {
            try
            {
                return _fieldRepository.CreateMultiObjectFieldOnDocument(Domain.Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME, sourceWorkspaceObjectTypeId);
            }
            catch (Exception e)
            {
                throw new Exception(RelativityProvider.ERROR_CREATE_SOURCE_CASE_FIELDS_ON_DESTINATION_CASE, e);
            }
        }

        public SourceWorkspaceDTO RetrieveForSourceWorkspaceId(int sourceWorkspaceArtifactId, string federatedInstanceName, int? federatedInstanceArtifactId)
        {
            QueryRequest queryRequest = CreateQueryRequestForRetrieveSourceWorkspace(sourceWorkspaceArtifactId, federatedInstanceName, federatedInstanceArtifactId);

            RelativityObject relativityObject = _objectManager.Query(queryRequest)?.FirstOrDefault();

            return relativityObject == null
                ? null
                : new SourceWorkspaceDTO(relativityObject.ArtifactID, relativityObject.FieldValues);
        }

        public int Create(SourceWorkspaceDTO sourceWorkspaceDto)
        {
            return _objectManager.Create(sourceWorkspaceDto.ObjectTypeRef, sourceWorkspaceDto.FieldRefValuePairs);
        }

        public void Update(SourceWorkspaceDTO sourceWorkspaceDto)
        {
            _objectManager.Update(sourceWorkspaceDto.ArtifactId, sourceWorkspaceDto.FieldRefValuePairs);
        }

        private static QueryRequest CreateQueryRequestForRetrieveSourceWorkspace(int sourceWorkspaceArtifactId, string federatedInstanceName, int? federatedInstanceArtifactId)
        {
            string condition = CreateConditionForRetrieveSourceWorkspace(sourceWorkspaceArtifactId, federatedInstanceName, federatedInstanceArtifactId);

            return new QueryRequest
            {
                ObjectType = new ObjectTypeRef
                {
                    Guid = SourceWorkspaceDTO.ObjectTypeGuid
                },
                Fields = new[]
                {
                    new FieldRef { Name = Domain.Constants.SOURCEWORKSPACE_CASEID_FIELD_NAME },
                    new FieldRef { Name = Domain.Constants.SOURCEWORKSPACE_CASENAME_FIELD_NAME },
                    new FieldRef { Name = Domain.Constants.SOURCEWORKSPACE_NAME_FIELD_NAME },
                    new FieldRef { Name = Domain.Constants.SOURCEWORKSPACE_INSTANCENAME_FIELD_NAME }
                },
                Condition = condition
            };
        }

        private static string CreateConditionForRetrieveSourceWorkspace(int sourceWorkspaceArtifactId, string federatedInstanceName, int? federatedInstanceArtifactId)
        {
            string sourceWorkspaceCondition = $"'{Domain.Constants.SOURCEWORKSPACE_CASEID_FIELD_NAME}' == {sourceWorkspaceArtifactId}";

            string instanceCondition;
            if (federatedInstanceArtifactId.HasValue)
            {
                instanceCondition = $"'{Domain.Constants.SOURCEWORKSPACE_INSTANCENAME_FIELD_NAME}' == '{federatedInstanceName}'";
            }
            else
            {
                string instanceNameCondition = $"'{Domain.Constants.SOURCEWORKSPACE_INSTANCENAME_FIELD_NAME}' == '{federatedInstanceName}'";
                string instanceNameIsNotSetCondition = $"NOT '{Domain.Constants.SOURCEWORKSPACE_INSTANCENAME_FIELD_NAME}' ISSET";
                instanceCondition = $"({instanceNameCondition}) OR ({instanceNameIsNotSetCondition})";
            }

            string condition = $"({sourceWorkspaceCondition}) AND ({instanceCondition})";
            return condition;
        }
    }
}