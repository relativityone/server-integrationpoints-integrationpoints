using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.UtilityDTO;
using Relativity;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.ScheduleQueue.Core.Validation
{
    public class UserExistsValidator : IJobPreValidator
    {
        private readonly IRelativityObjectManagerFactory _relativityObjectManagerFactory;

        public UserExistsValidator(IRelativityObjectManagerFactory relativityObjectManagerFactory)
        {
            _relativityObjectManagerFactory = relativityObjectManagerFactory;
        }

        public async Task<PreValidationResult> ValidateAsync(Job job)
        {
            IRelativityObjectManager relativityObjectManager = _relativityObjectManagerFactory.CreateRelativityObjectManager(-1);

            var request = new QueryRequest
            {
                ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.User },
                Condition = $"'ArtifactID' == {job.SubmittedBy}",
            };

            ResultSet<RelativityObjectSlim> result = await relativityObjectManager.QuerySlimAsync(request, 0, 1, executionIdentity: ExecutionIdentity.System).ConfigureAwait(false);

            if (result.TotalCount > 0)
            {
                return PreValidationResult.Success;
            }

            return PreValidationResult.InvalidJob(
                $"User (userId - {job.SubmittedBy}) who scheduled the job no longer exists, so the job schedule will be cancelled. " +
                $"To enable the schedule again, edit the Integration Point and on Save schedule will be restored",
                true,
                false);
        }
    }
}
