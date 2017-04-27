using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class SourceWorkspaceManager : ISourceWorkspaceManager
	{
		private readonly IAPILog _logger;
		private readonly IRepositoryFactory _repositoryFactory;
		private ISourceWorkspaceRepository _sourceWorkspaceRepository;

		public SourceWorkspaceManager(IRepositoryFactory repositoryFactory, IHelper helper)
		{
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<SourceWorkspaceManager>();
			_repositoryFactory = repositoryFactory;
		}

		public SourceWorkspaceDTO CreateSourceWorkspaceDto(int destinationWorkspaceArtifactId, int sourceWorkspaceArtifactId, int? federatedInstanceArtifactId,
			int sourceWorkspaceDescriptorArtifactTypeId)
		{
			_sourceWorkspaceRepository = _repositoryFactory.GetSourceWorkspaceRepository(destinationWorkspaceArtifactId);
			IWorkspaceRepository workspaceRepository = _repositoryFactory.GetSourceWorkspaceRepository();
			WorkspaceDTO workspaceDto = workspaceRepository.Retrieve(sourceWorkspaceArtifactId);

			string currentInstanceName = FederatedInstanceManager.LocalInstance.Name;
			if (federatedInstanceArtifactId.HasValue)
			{
				IInstanceSettingRepository instanceSettingRepository = _repositoryFactory.GetInstanceSettingRepository();
				currentInstanceName = instanceSettingRepository.GetConfigurationValue("Relativity.Authentication", "FriendlyInstanceName");
			}
			string sourceWorkspaceName = GenerateSourceWorkspaceName(currentInstanceName, workspaceDto.Name, sourceWorkspaceArtifactId);

			SourceWorkspaceDTO sourceWorkspaceDto = _sourceWorkspaceRepository.RetrieveForSourceWorkspaceId(sourceWorkspaceArtifactId, currentInstanceName, federatedInstanceArtifactId);

			if (sourceWorkspaceDto == null)
			{
				return CreateSourceWorkspaceDto(sourceWorkspaceArtifactId, sourceWorkspaceDescriptorArtifactTypeId, sourceWorkspaceName, workspaceDto.Name, currentInstanceName);
			}
			return UpdateSourceWorkspaceDto(sourceWorkspaceDto, sourceWorkspaceDescriptorArtifactTypeId, workspaceDto.Name, sourceWorkspaceName, currentInstanceName);
		}

		private string GenerateSourceWorkspaceName(string instanceName, string workspaceName, int workspaceArtifactId)
		{
			var name = Utils.GetFormatForWorkspaceOrJobDisplay(instanceName, workspaceName, workspaceArtifactId);
			if (name.Length > Data.Constants.DEFAULT_NAME_FIELD_LENGTH)
			{
				_logger.LogWarning("Relativity Source Case Name exceeded max length and has been shortened. Full name {name}.", name);

				int overflow = name.Length - Data.Constants.DEFAULT_NAME_FIELD_LENGTH;
				var trimmedInstanceName = instanceName.Substring(0, instanceName.Length - overflow);
				name = Utils.GetFormatForWorkspaceOrJobDisplay(trimmedInstanceName, workspaceName, workspaceArtifactId);
			}
			return name;
		}

		private SourceWorkspaceDTO CreateSourceWorkspaceDto(int sourceWorkspaceArtifactId, int sourceWorkspaceDescriptorArtifactTypeId, string sourceWorkspaceName,
			string workspaceName, string currentInstanceName)
		{
			var sourceWorkspaceDto = new SourceWorkspaceDTO
			{
				ArtifactId = -1,
				ArtifactTypeId = sourceWorkspaceDescriptorArtifactTypeId,
				Name = sourceWorkspaceName,
				SourceCaseArtifactId = sourceWorkspaceArtifactId,
				SourceCaseName = workspaceName,
				SourceInstanceName = currentInstanceName
			};

			int artifactId = _sourceWorkspaceRepository.Create(sourceWorkspaceDto);
			sourceWorkspaceDto.ArtifactId = artifactId;
			return sourceWorkspaceDto;
		}

		private SourceWorkspaceDTO UpdateSourceWorkspaceDto(SourceWorkspaceDTO sourceWorkspaceDto, int sourceWorkspaceDescriptorArtifactTypeId, string workspaceName,
			string sourceWorkspaceName, string currentInstanceName)
		{
			sourceWorkspaceDto.ArtifactTypeId = sourceWorkspaceDescriptorArtifactTypeId;
			// Check to see if instance should be updated
			if (sourceWorkspaceDto.SourceCaseName != workspaceName || sourceWorkspaceDto.SourceInstanceName != currentInstanceName)
			{
				sourceWorkspaceDto.Name = sourceWorkspaceName;
				sourceWorkspaceDto.SourceCaseName = workspaceName;
				sourceWorkspaceDto.SourceInstanceName = currentInstanceName;
				_sourceWorkspaceRepository.Update(sourceWorkspaceDto);
			}
			return sourceWorkspaceDto;
		}
	}
}