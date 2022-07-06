using kCura.IntegrationPoints.Data;
using Relativity;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using System.Threading.Tasks;

namespace kCura.ScheduleQueue.Core.Validation
{
    public class WorkspaceExistsValidator : IJobPreValidator
    {
		private readonly IHelper _helper;

        public WorkspaceExistsValidator(IHelper helper)
        {
            _helper = helper;
        }

        public async Task<PreValidationResult> ValidateAsync(Job job)
        {
			using (IObjectManager proxy = _helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System))
			{
				var request = new QueryRequest()
				{
					ObjectType = new ObjectTypeRef() { ArtifactTypeID = (int)ArtifactType.Case },
					Condition = $"'ArtifactID' == {job.WorkspaceID}",
				};

				QueryResultSlim result = await proxy.QuerySlimAsync(-1, request, 0, 1).ConfigureAwait(false);

				return result.TotalCount > 0
					? PreValidationResult.Success
					: PreValidationResult.InvalidJob($"Workspace {job.WorkspaceID} does not exist anymore", false);
			}
		}
    }
}
