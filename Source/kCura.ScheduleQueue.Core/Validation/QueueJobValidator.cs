using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using Relativity;
using Relativity.API;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace kCura.ScheduleQueue.Core.Validation
{
	public class QueueJobValidator : IQueueJobValidator
	{
		private readonly IHelper _helper;

		public QueueJobValidator(IHelper helper)
		{
			_helper = helper;
		}

		public async Task<ValidationResult> ValidateAsync(Job job)
		{
            ValidationResult validationResult = await ValidateWorkspaceExistsAsync(job.WorkspaceID).ConfigureAwait(false);
			if (!validationResult.IsValid)
			{
				return validationResult;
			}

            validationResult = await ValidateUserExistsAsync(job.SubmittedBy).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return validationResult;
            }

			validationResult = await ValidateIntegrationPointExistsAsync(job.RelatedObjectArtifactID, job.WorkspaceID).ConfigureAwait(false);

			return validationResult;
		}

		private async Task<ValidationResult> ValidateWorkspaceExistsAsync(int workspaceId)
		{
			using (IObjectManager proxy = _helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System))
			{
				var request = new QueryRequest()
				{
					ObjectType = new ObjectTypeRef() { ArtifactTypeID = (int)ArtifactType.Case },
					Condition = $"'ArtifactID' == {workspaceId}",
				};

				QueryResultSlim result = await proxy.QuerySlimAsync(-1, request, 0, 1).ConfigureAwait(false);

				return result.TotalCount > 0 ? ValidationResult.Success : ValidationResult.Failed($"Workspace {workspaceId} does not exist anymore");
			}
		}

		private async Task<ValidationResult> ValidateIntegrationPointExistsAsync(int integrationPointId, int workspaceId)
		{
			using (IObjectManager proxy = _helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System))
			{
				var request = new ReadRequest()
				{
					Object = new RelativityObjectRef() { ArtifactID = integrationPointId }
				};

				ReadResult result = await proxy.ReadAsync(workspaceId, request).ConfigureAwait(false);

				return result.Object != null ? ValidationResult.Success : ValidationResult.Failed($"Integration Point {integrationPointId} does not exist anymore");
			}
		}

		private async Task<ValidationResult> ValidateUserExistsAsync(int userId)
        {
            using (IObjectManager proxy = _helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System))
            {
                var request = new QueryRequest()
                {
                    ObjectType = new ObjectTypeRef() { ArtifactTypeID = (int)ArtifactType.User },
                    Condition = $"'ArtifactID' == {userId}",
                };

                QueryResultSlim result = await proxy.QuerySlimAsync(-1, request, 0, 1).ConfigureAwait(false);

                if (result.TotalCount > 0)
                {
                    return ValidationResult.Success;
                }
                
                throw new NotFoundException($"User (userId - {userId}) who scheduled the job no longer exists, so the job schedule will be cancelled. To enable the schedule again, edit the Integration Point and on Save schedule will be restored");
            }
		}
	}
}
