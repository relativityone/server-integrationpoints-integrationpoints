using Relativity.API;
using System;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Executors
{
	internal sealed class DestinationWorkspaceTagLinker : IDestinationWorkspaceTagsLinker
	{
		private readonly IRdoGuidConfiguration _rdoGuidConfiguration;
		private readonly ISourceServiceFactoryForUser _serviceFactoryForUser;
		private readonly IAPILog _logger;

		public DestinationWorkspaceTagLinker(IRdoGuidConfiguration rdoGuidConfiguration, ISourceServiceFactoryForUser serviceFactoryForUser, IAPILog logger)
		{
			_rdoGuidConfiguration = rdoGuidConfiguration;
			_serviceFactoryForUser = serviceFactoryForUser;
			_logger = logger;
		}
		
		public async Task LinkDestinationWorkspaceTagToJobHistoryAsync(int sourceWorkspaceArtifactId, int destinationWorkspaceTagArtifactId, int jobArtifactId)
		{
			_logger.LogVerbose("Linking destination workspace tag Artifact ID: {destinationWorkspaceTagArtifactId} to job history Artifact ID: {jobArtifactId}");
			UpdateRequest request = CreateUpdateRequest(destinationWorkspaceTagArtifactId, jobArtifactId);

			using (IObjectManager objectManager = await _serviceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
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
							Guid = _rdoGuidConfiguration.JobHistory.DestinationWorkspaceInformationGuid
						},
						Value = new[] { destinationWorkspaceObjectValue }
					}
				}
			};
			return request;
		}
	}
}
