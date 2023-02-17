using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Utils;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
    public class DestinationWorkspaceRepository : IDestinationWorkspaceRepository
    {
        private readonly IRelativityObjectManager _objectManager;

        public DestinationWorkspaceRepository(IRelativityObjectManager objectManager)
        {
            _objectManager = objectManager;
        }

        public DestinationWorkspace Query(int targetWorkspaceArtifactId, int? federatedInstanceArtifactId)
        {
            string instanceCondition = federatedInstanceArtifactId.HasValue
                ? $"'{DestinationWorkspaceFields.DestinationInstanceArtifactID}' == {federatedInstanceArtifactId}"
                : $"(NOT '{DestinationWorkspaceFields.DestinationInstanceArtifactID}' ISSET)";

            var queryRequest = new QueryRequest
            {
                Condition = $"'{DestinationWorkspaceFields.DestinationWorkspaceArtifactID}' == {targetWorkspaceArtifactId} AND {instanceCondition}",
                Fields = new List<FieldRef>
                {
                    new FieldRef {Name = "ArtifactId" },
                    new FieldRef {Guid = new Guid(DestinationWorkspaceFieldGuids.DestinationWorkspaceName) },
                    new FieldRef {Guid = new Guid(DestinationWorkspaceFieldGuids.DestinationInstanceName) },
                    new FieldRef {Guid = new Guid(DestinationWorkspaceFieldGuids.DestinationWorkspaceArtifactID) },
                    new FieldRef {Guid = new Guid(DestinationWorkspaceFieldGuids.DestinationInstanceArtifactID) },
                    new FieldRef {Guid = new Guid(DestinationWorkspaceFieldGuids.Name)}
                }
            };

            return _objectManager
                .Query<DestinationWorkspace>(queryRequest)
                .FirstOrDefault();
        }

        public DestinationWorkspace Create(int targetWorkspaceArtifactId, string targetWorkspaceName, int? federatedInstanceArtifactId, string federatedInstanceName)
        {
            string instanceName = WorkspaceAndJobNameUtils.GetFormatForWorkspaceOrJobDisplay(federatedInstanceName, targetWorkspaceName, targetWorkspaceArtifactId);

            var destinationWorkspace = new DestinationWorkspace
            {
                DestinationWorkspaceArtifactID = targetWorkspaceArtifactId,
                DestinationWorkspaceName = targetWorkspaceName,
                DestinationInstanceName = federatedInstanceName,
                DestinationInstanceArtifactID = federatedInstanceArtifactId,
                Name = instanceName
            };

            int artifactId = _objectManager.Create(destinationWorkspace);
            destinationWorkspace.ArtifactId = artifactId;
            return destinationWorkspace;
        }

        public void Update(DestinationWorkspace destinationWorkspace)
        {
            string instanceName = WorkspaceAndJobNameUtils.GetFormatForWorkspaceOrJobDisplay(destinationWorkspace.DestinationInstanceName, destinationWorkspace.DestinationWorkspaceName,
                destinationWorkspace.DestinationWorkspaceArtifactID);
            destinationWorkspace.Name = instanceName;

            _objectManager.Update(destinationWorkspace);
        }

        public void LinkDestinationWorkspaceToJobHistory(int destinationWorkspaceInstanceId, int jobHistoryInstanceId)
        {
            var destinationWorkspaceObjectValue = new RelativityObjectValue
            {
                ArtifactID = destinationWorkspaceInstanceId
            };
            var fieldsToUpdate = new List<FieldRefValuePair>
            {
                new FieldRefValuePair
                {
                    Field = new FieldRef
                    {
                        Guid = new Guid(JobHistoryFieldGuids.DestinationWorkspaceInformation)
                    },
                    Value = new[] { destinationWorkspaceObjectValue }
                }
            };

            bool isUpdated;
            try
            {
                isUpdated = _objectManager.Update(jobHistoryInstanceId, fieldsToUpdate);
            }
            catch (Exception e)
            {
                throw new IntegrationPointsException(TaggingErrors.LINK_OBJECT_INSTANCE_ERROR, e);
            }

            if (!isUpdated)
            {
                throw new IntegrationPointsException(TaggingErrors.LINK_OBJECT_INSTANCE_ERROR);
            }
        }
    }
}
