using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Exceptions;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Services
{
    public class ArtifactService : IArtifactService
    {
        private readonly IRelativityObjectManagerFactory _objectManagerFactory;
        private readonly IAPILog _logger;

        public ArtifactService(IRelativityObjectManagerFactory objectManagerFactory, IHelper helper)
        {
            _objectManagerFactory = objectManagerFactory;
            _logger = helper.GetLoggerFactory().GetLogger().ForContext<ArtifactService>();
        }

        public RelativityObject GetArtifact(int workspaceArtifactId, string artifactTypeName, int artifactId)
        {
            return GetArtifactsByCondition(workspaceArtifactId, artifactTypeName, $"'ArtifactID' == {artifactId}").SingleOrDefault();
        }

        private IEnumerable<RelativityObject> GetArtifactsByCondition(int workspaceArtifactId, string artifactTypeName, string condition = null)
        {
            QueryRequest queryRequest = new QueryRequest()
            {
                ObjectType = new ObjectTypeRef()
                {
                    Name = artifactTypeName
                },
                Condition = condition
            };

            try
            {
                int actualWorkspaceId = workspaceArtifactId == 0 ? -1 : workspaceArtifactId;

                IRelativityObjectManager objectManager = _objectManagerFactory.CreateRelativityObjectManager(actualWorkspaceId);
                List<RelativityObject> queryResult = objectManager.QueryAsync(queryRequest).GetAwaiter().GetResult();
                return queryResult;
            }
            catch (Exception ex)
            {
                LogQueryingArtifactError(artifactTypeName, ex.Message);
                throw new IntegrationPointsException($"Artifact query failed: {ex.Message}")
                {
                    ExceptionSource = IntegrationPointsExceptionSource.KEPLER
                };
            }
        }
        #region Logging

        private void LogQueryingArtifactError(string artifactTypeName, string message)
        {
            _logger.LogError("Artifact query failed for {ArtifactTypeName}. Details: {Message}", artifactTypeName, message);
        }

        #endregion
    }
}
