using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Data.Transformer
{
	public static class IntegrationPointTransformer
	{
		/// <summary>
		/// Converts IntegrationPoint object to DTO format
		/// </summary>
		/// <param name="integrationPoint">IntegrationPoint object to be transformed</param>
		/// <returns>IntegrationPoint object in DTO form</returns>
		public static IntegrationPointDTO ToDto(this IntegrationPoint integrationPoint)
		{
			var dto = new IntegrationPointDTO()
			{
				ArtifactId = integrationPoint.ArtifactId,
				DestinationConfiguration  = integrationPoint.DestinationConfiguration,
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
				OverwriteFields = integrationPoint.OverwriteFields.ConvertToChoiceValue(IntegrationPointDTO.Choices.OverwriteFields.GuidValues),
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
		public static List<IntegrationPointDTO> ToDto(this IEnumerable<IntegrationPoint> integrationPoints)
		{
			return integrationPoints.Select(integrationPoint => integrationPoint.ToDto()).ToList();
		}
	}
}
