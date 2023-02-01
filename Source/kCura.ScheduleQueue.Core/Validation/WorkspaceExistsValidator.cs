using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.UtilityDTO;
using Relativity;
using Relativity.Services.Objects.DataContracts;

namespace kCura.ScheduleQueue.Core.Validation
{
    public class WorkspaceExistsValidator : IJobPreValidator
    {
        private readonly IRelativityObjectManagerFactory _relativityObjectManagerFactory;

        public WorkspaceExistsValidator(IRelativityObjectManagerFactory relativityObjectManagerFactory)
        {
            _relativityObjectManagerFactory = relativityObjectManagerFactory;
        }

        public async Task<PreValidationResult> ValidateAsync(Job job)
        {
            IRelativityObjectManager relativityObjectManager = _relativityObjectManagerFactory.CreateRelativityObjectManager(-1);

            var request = new QueryRequest
            {
                ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.Case },
                Condition = $"'ArtifactID' == {job.WorkspaceID}",
            };

            ResultSet<RelativityObjectSlim> result = await relativityObjectManager.QuerySlimAsync(request, 0, 1).ConfigureAwait(false);

            return result.TotalCount > 0
                ? PreValidationResult.Success
                : PreValidationResult.InvalidJob(
                    $"Workspace {job.WorkspaceID} does not exist anymore",
                    false,
                    false);
        }
    }
}
