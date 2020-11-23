using System.Threading.Tasks;
using kCura.Relativity.Client;
using Relativity.API;
using Relativity.Services.Interfaces.Workspace;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using ReadResult = Relativity.Services.Objects.DataContracts.ReadResult;

namespace kCura.ScheduleQueue.Core.Validation
{
	public class QueueJobValidator
	{
		private ValidationResult _validationResult = ValidationResult.Success;

		private readonly IHelper _helper;
		private readonly Job _job;

		public QueueJobValidator(Job job, IHelper helper)
		{
			_job = job;
			_helper = helper;
		}

		public async Task<ValidationResult> ValidateAsync()
		{
			await ValidateWorkspaceExistsAsync().ConfigureAwait(false);
			await ValidateIntegrationPointExistsAsync().ConfigureAwait(false);

			return _validationResult;
		}

		private async Task ValidateWorkspaceExistsAsync()
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
					Condition = $"'ArtifactID' == {_job.WorkspaceID}",
				};

				QueryResultSlim result = await proxy.QuerySlimAsync(-1, request, 0, 1).ConfigureAwait(false);

				_validationResult = result.TotalCount > 0
					? ValidationResult.Success : ValidationResult.Failed($"Workspace {_job.WorkspaceID} does not exist anymore");
			}
		}

		private async Task ValidateIntegrationPointExistsAsync()
		{
			if (!_validationResult.IsValid)
			{
				return;
			}

			using (IObjectManager proxy = _helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System))
			{
				var request = new ReadRequest()
				{
					Object = new RelativityObjectRef() { ArtifactID = _job.RelatedObjectArtifactID }
				};

				ReadResult result = await proxy.ReadAsync(_job.WorkspaceID, request).ConfigureAwait(false);

				_validationResult = result.Object != null
					? ValidationResult.Success : ValidationResult.Failed($"Integration Point {_job.RelatedObjectArtifactID} does not exist anymore");
			}
		}
	}
}
