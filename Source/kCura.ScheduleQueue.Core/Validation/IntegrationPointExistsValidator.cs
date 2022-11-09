using kCura.IntegrationPoints.Data;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using System.Threading.Tasks;

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
                var request = new ReadRequest()
                {
                    Object = new RelativityObjectRef() { ArtifactID = job.RelatedObjectArtifactID }
                };

                ReadResult result = await proxy.ReadAsync(job.WorkspaceID, request).ConfigureAwait(false);

                return result.Object != null
                    ? PreValidationResult.Success
                    : PreValidationResult.InvalidJob(
                        $"Integration Point {job.RelatedObjectArtifactID} does not exist anymore",
                        false,
                        false);
            }
        }
    }
}
