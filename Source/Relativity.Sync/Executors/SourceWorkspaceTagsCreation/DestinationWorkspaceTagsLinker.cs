using System;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Executors.SourceWorkspaceTagsCreation
{
	internal sealed class DestinationWorkspaceTagsLinker : IDestinationWorkspaceTagsLinker
	{
		private readonly Guid DestinationWorkspaceInformationGuid = new Guid("20a24c4e-55e8-4fc2-abbe-f75c07fad91b");

		private readonly ISourceServiceFactoryForUser _sourceServiceFactoryForUser;
		private readonly IAPILog _logger;

		public DestinationWorkspaceTagsLinker(ISourceServiceFactoryForUser sourceServiceFactoryForUser, IAPILog logger)
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
				await objectManager.UpdateAsync(sourceWorkspaceArtifactId, request).ConfigureAwait(false);
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
						Value = new[] {destinationWorkspaceObjectValue}
					}
				}
			};
			return request;
		}
	}
}