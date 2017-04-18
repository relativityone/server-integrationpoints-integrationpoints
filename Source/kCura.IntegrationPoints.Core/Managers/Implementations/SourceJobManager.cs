using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class SourceJobManager : ISourceJobManager
	{
		private readonly IRepositoryFactory _repositoryFactory;

		public SourceJobManager(IRepositoryFactory repositoryFactory)
		{
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
				Name = Utils.GetFormatForWorkspaceOrJobDisplay(jobHistoryRdo.TextIdentifier, jobHistoryArtifactId),
				SourceWorkspaceArtifactId = sourceWorkspaceRdoInstanceArtifactId,
				JobHistoryArtifactId = jobHistoryArtifactId,
				JobHistoryName = jobHistoryRdo.TextIdentifier
			};

			int artifactId = sourceJobRepository.Create(jobHistoryDto);
			jobHistoryDto.ArtifactId = artifactId;

			return jobHistoryDto;
		}
	}
}