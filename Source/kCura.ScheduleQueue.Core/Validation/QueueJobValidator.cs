using System.Threading.Tasks;
using kCura.Relativity.Client;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using ReadResult = Relativity.Services.Objects.DataContracts.ReadResult;

namespace kCura.ScheduleQueue.Core.Validation
{
	public class QueueJobValidator : IQueueJobValidator
	{
		private ValidationResult _validationResult = ValidationResult.Success;

		private readonly IHelper _helper;

		public QueueJobValidator(IHelper helper)
		{
			_helper = helper;
		}

		public async Task<ValidationResult> ValidateAsync(Job job)
		{
			await ValidateWorkspaceExistsAsync(job.WorkspaceID).ConfigureAwait(false);
			await ValidateIntegrationPointExistsAsync(job.RelatedObjectArtifactID, job.WorkspaceID).ConfigureAwait(false);

			return _validationResult;
		}

		private async Task ValidateWorkspaceExistsAsync(int workspaceId)
		{
			if (!_validationResult.IsValid)
			{
				return;
			}

			using (IObjectManager proxy = _helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System))
			{
				var request = new QueryRequest()
				{
					ObjectType = new ObjectTypeRef() { ArtifactTypeID = (int)ArtifactType.Case },
					Condition = $"'ArtifactID' == {workspaceId}",
				};

				QueryResultSlim result = await proxy.QuerySlimAsync(-1, request, 0, 1).ConfigureAwait(false);

				_validationResult = result.TotalCount > 0
					? ValidationResult.Success : ValidationResult.Failed($"Workspace {workspaceId} does not exist anymore");
			}
		}

		private async Task ValidateIntegrationPointExistsAsync(int integrationPointId, int workspaceId)
		{
			if (!_validationResult.IsValid)
			{
				return;
			}

			using (IObjectManager proxy = _helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System))
			{
				var request = new ReadRequest()
				{
					Object = new RelativityObjectRef() { ArtifactID = integrationPointId }
				};

				ReadResult result = await proxy.ReadAsync(workspaceId, request).ConfigureAwait(false);

				_validationResult = result.Object != null
					? ValidationResult.Success : ValidationResult.Failed($"Integration Point {integrationPointId} does not exist anymore");
			}
		}
	}
}
