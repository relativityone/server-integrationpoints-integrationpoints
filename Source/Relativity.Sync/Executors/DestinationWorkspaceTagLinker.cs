using System;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Executors
{
	internal sealed class DestinationWorkspaceTagLinker : IDestinationWorkspaceTagsLinker
	{
		private readonly ISourceServiceFactoryForUser _sourceServiceFactoryForUser;
		private readonly IAPILog _logger;

		private static readonly Guid DestinationWorkspaceInformationGuid = new Guid("20a24c4e-55e8-4fc2-abbe-f75c07fad91b");

		public DestinationWorkspaceTagLinker(ISourceServiceFactoryForUser sourceServiceFactoryForUser, IAPILog logger)
		{
			_sourceServiceFactoryForUser = sourceServiceFactoryForUser;
			_logger = logger;
		}
		
		public async Task LinkDestinationWorkspaceTagToJobHistoryAsync(int sourceWorkspaceArtifactId, int destinationWorkspaceTagArtifactId, int jobArtifactId)
		{
			_logger.LogVerbose("Linking destination workspace tag Artifact ID: {destinationWorkspaceTagArtifactId} to job history Artifact ID: {jobArtifactId}");
			UpdateRequest request = CreateUpdateRequest(destinationWorkspaceTagArtifactId, jobArtifactId);

			using (IObjectManager objectManager = await _sourceServiceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				try
				{
					await objectManager.UpdateAsync(sourceWorkspaceArtifactId, request).ConfigureAwait(false);
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, $"Failed to link {nameof(DestinationWorkspaceTag)} to Job History: {{request}}", request);
					throw new DestinationWorkspaceTagsLinkerException($"Failed to link {nameof(DestinationWorkspaceTag)} to Job History", ex);
				}
			}
		}

		private UpdateRequest CreateUpdateRequest(int destinationWorkspaceTagArtifactId, int jobArtifactId)
		{
			RelativityObjectValue destinationWorkspaceObjectValue = new RelativityObjectValue()
			{
				ArtifactID = destinationWorkspaceTagArtifactId
			};

			UpdateRequest request = new UpdateRequest
			{
				Object = new RelativityObjectRef {ArtifactID = jobArtifactId },
				FieldValues = new[]
				{
					new FieldRefValuePair()
					{
						Field = new FieldRef()
						{
							Guid = DestinationWorkspaceInformationGuid
						},
						Value = new[] { destinationWorkspaceObjectValue }
					}
				}
			};
			return request;
		}
	}
}