using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Transformers
{
	public class IntegrationPointTransformer : IDtoTransformer<IntegrationPointDTO, IntegrationPoint>
	{
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly int _workspaceArtifactId;

		public IntegrationPointTransformer(IHelper helper, int workspaceArtifactId)
			: this(new RepositoryFactory(helper, helper.GetServicesManager()), workspaceArtifactId)
		{
		}

		/// <summary>
		/// Only external usage of this constructor should be unit tests
		/// </summary>
		internal IntegrationPointTransformer(IRepositoryFactory repositoryFactory, int workspaceArtifactId)
		{
			_repositoryFactory = repositoryFactory;
			_workspaceArtifactId = workspaceArtifactId;
		}

		//public IntegrationPointRepository(, int workspaceArtifactId)
		/// <summary>
		/// Converts IntegrationPoint object to DTO format
		/// </summary>
		/// <param name="integrationPoint">IntegrationPoint object to be transformed</param>
		/// <returns>IntegrationPoint object in DTO form</returns>
		public IntegrationPointDTO ConvertToDto(IntegrationPoint integrationPoint)
		{
			IArtifactGuidRepository artifactGuidRepository = _repositoryFactory.GetArtifactGuidRepository(_workspaceArtifactId);
			int overwriteFieldsChoiceArtifactId = integrationPoint.OverwriteFields.ArtifactID;
			Guid overwriteFieldsChoiceGuid =
				artifactGuidRepository.GetGuidsForArtifactIds(new List<int> {overwriteFieldsChoiceArtifactId})[
					overwriteFieldsChoiceArtifactId];

			var dto = new IntegrationPointDTO()
			{
				ArtifactId = integrationPoint.ArtifactId,
				DestinationConfiguration = integrationPoint.DestinationConfiguration,
				DestinationProvider = integrationPoint.DestinationProvider,
				EmailNotificationRecipients = integrationPoint.EmailNotificationRecipients,
				EnableScheduler = integrationPoint.EnableScheduler,
				FieldMappings = integrationPoint.FieldMappings,
				HasErrors = integrationPoint.HasErrors,
				JobHistory = integrationPoint.JobHistory,
				LastRuntimeUTC = integrationPoint.LastRuntimeUTC,
				LogErrors = integrationPoint.LogErrors,
				Name = integrationPoint.Name,
				NextScheduledRuntimeUTC = integrationPoint.NextScheduledRuntimeUTC,
				OverwriteFields = IntegrationPointDTO.Choices.OverwriteFields.GuidValues[overwriteFieldsChoiceGuid],
				ScheduleRule = integrationPoint.ScheduleRule,
				SourceConfiguration = integrationPoint.SourceConfiguration,
				SourceProvider = integrationPoint.SourceProvider
			};
			return dto;
		}

		/// <summary>
		/// Converts IntegrationPoint objects to DTO format
		/// </summary>
		/// <param name="integrationPoints">IntegrationPoint objects to be transformed</param>
		/// <returns>List of IntegrationPoint objects in DTO form</returns>
		public List<IntegrationPointDTO> ConvertToDto(IEnumerable<IntegrationPoint> integrationPoints)
		{
			return integrationPoints.Select(ConvertToDto).ToList();
		}

		/// <summary>
		/// Converts ArtifactDTO object to IntegrationPointDTO format
		/// </summary>
		/// <param name="integrationPoint">ArtifactDTO object to be transformed</param>
		/// <returns>List of IntegrationPoint objects in DTO form</returns>
		public IntegrationPointDTO ConvertArtifactDtoToDto(ArtifactDTO integrationPoint)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Converts ArtifactDTO objects to JobHistoryErrorDTO format
		/// </summary>
		/// <param name="integrationPoints">ArtifactDTO objects to be transformed</param>
		/// <returns>List of JobHistoryError objects in DTO form</returns>
		public List<IntegrationPointDTO> ConvertArtifactDtoToDto(IEnumerable<ArtifactDTO> integrationPoints)
		{
			return integrationPoints.Select(ConvertArtifactDtoToDto).ToList();
		}
	}
}
