using kCura.IntegrationPoints.Data;
using Relativity;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using System.Threading.Tasks;

namespace kCura.ScheduleQueue.Core.Validation
{
    public class UserExistsValidator : IJobPreValidator
    {
        private readonly IHelper _helper;

        public UserExistsValidator(IHelper helper)
        {
            _helper = helper;
        }

        public async Task<PreValidationResult> ValidateAsync(Job job)
        {
            using (IObjectManager proxy = _helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System))
            {
                var request = new QueryRequest()
                {
                    ObjectType = new ObjectTypeRef() { ArtifactTypeID = (int)ArtifactType.User },
                    Condition = $"'ArtifactID' == {job.SubmittedBy}",
                };

                QueryResultSlim result = await proxy.QuerySlimAsync(-1, request, 0, 1).ConfigureAwait(false);

                if (result.TotalCount > 0)
                {
                    return PreValidationResult.Success;
                }

                return result.TotalCount > 0 ? PreValidationResult.Success : PreValidationResult.InvalidJob(
                    $"User (userId - {job.SubmittedBy}) who scheduled the job no longer exists, so the job schedule will be cancelled. " +
                    $"To enable the schedule again, edit the Integration Point and on Save schedule will be restored", true);
            }
        }
    }
}
