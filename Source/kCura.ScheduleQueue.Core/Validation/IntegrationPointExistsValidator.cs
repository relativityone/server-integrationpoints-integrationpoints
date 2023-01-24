using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.UtilityDTO;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.ScheduleQueue.Core.Validation
{
    public class IntegrationPointExistsValidator : IJobPreValidator
    {
        private readonly IRelativityObjectManagerFactory _relativityObjectManagerFactory;

        public IntegrationPointExistsValidator(IRelativityObjectManagerFactory relativityObjectManagerFactory)
        {
            _relativityObjectManagerFactory = relativityObjectManagerFactory;
        }

        public async Task<PreValidationResult> ValidateAsync(Job job)
        {
            IRelativityObjectManager relativityObjectManager = _relativityObjectManagerFactory.CreateRelativityObjectManager(job.WorkspaceID);
            var request = new QueryRequest
            {
                ObjectType = new ObjectTypeRef { Guid = ObjectTypeGuids.IntegrationPointGuid },
                Condition = $"'ArtifactID' == {job.RelatedObjectArtifactID}"
            };

            ResultSet<RelativityObject> result = await relativityObjectManager.QueryAsync(request, 0, 1, executionIdentity: ExecutionIdentity.System)
                .ConfigureAwait(false);

            if (result.TotalCount > 0)
            {
                return PreValidationResult.Success;
            }

            return PreValidationResult.InvalidJob(
                    $"Integration Point {job.RelatedObjectArtifactID} does not exist anymore",
                    false,
                    false);
        }
    }
}
