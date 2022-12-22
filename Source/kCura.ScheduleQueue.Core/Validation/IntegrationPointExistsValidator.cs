using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace kCura.ScheduleQueue.Core.Validation
{
    public class IntegrationPointExistsValidator : IJobPreValidator
    {
        private readonly IHelper _helper;

        public IntegrationPointExistsValidator(IHelper helper)
        {
            _helper = helper;
        }

        public async Task<PreValidationResult> ValidateAsync(Job job)
        {
            using (IObjectManager proxy = _helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System))
            {
                var request = new QueryRequest()
                {
                    ObjectType = new ObjectTypeRef { Guid = ObjectTypeGuids.IntegrationPointGuid },
                    Condition = $"'ArtifactID' == {job.RelatedObjectArtifactID}"
                };

                QueryResult result = await proxy.QueryAsync(job.WorkspaceID, request, 0, 1)
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
}
