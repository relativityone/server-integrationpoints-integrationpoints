using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client.DTOs;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class SourceJobManager : ISourceJobManager
	{
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IAPILog _logger;

		public SourceJobManager(IRepositoryFactory repositoryFactory, IHelper helper)
		{
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<SourceJobManager>();
			_repositoryFactory = repositoryFactory;
		}

		public SourceJobDTO CreateSourceJobDto(int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId, int jobHistoryArtifactId, int sourceWorkspaceRdoInstanceArtifactId,
			int sourceJobDescriptorArtifactTypeId)
		{
			ISourceJobRepository sourceJobRepository = _repositoryFactory.GetSourceJobRepository(destinationWorkspaceArtifactId);
			IRdoRepository rdoRepository = _repositoryFactory.GetRdoRepository(sourceWorkspaceArtifactId);

			RDO jobHistoryRdo = rdoRepository.ReadSingle(jobHistoryArtifactId);
			var jobHistoryDto = new SourceJobDTO
			{
				ArtifactTypeId = sourceJobDescriptorArtifactTypeId,
				Name = GenerateSourceJobName(jobHistoryRdo.TextIdentifier, jobHistoryArtifactId),
				SourceWorkspaceArtifactId = sourceWorkspaceRdoInstanceArtifactId,
				JobHistoryArtifactId = jobHistoryArtifactId,
				JobHistoryName = jobHistoryRdo.TextIdentifier
			};

			int artifactId = sourceJobRepository.Create(jobHistoryDto);
			jobHistoryDto.ArtifactId = artifactId;

			return jobHistoryDto;
		}

		private string GenerateSourceJobName(string jobHistoryName, int jobHistoryArtifactId)
		{
			var name = Utils.GetFormatForWorkspaceOrJobDisplay(jobHistoryName, jobHistoryArtifactId);
			if (name.Length > Data.Constants.DEFAULT_NAME_FIELD_LENGTH)
			{
				_logger.LogWarning("Relativity Source Job Name exceeded max length and has been shortened. Full name {name}.", name);

				int overflow = name.Length - Data.Constants.DEFAULT_NAME_FIELD_LENGTH;
				var trimmedJobHistoryName = jobHistoryName.Substring(0, jobHistoryName.Length - overflow);
				name = Utils.GetFormatForWorkspaceOrJobDisplay(trimmedJobHistoryName, jobHistoryArtifactId);
			}
			return name;
		}
	}
}