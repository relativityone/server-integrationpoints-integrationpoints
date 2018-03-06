using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.Exporter.Validators
{
	public class DestinationFieldsValidator
	{
		private readonly IAPILog _logger;
		private readonly IFieldQueryRepository _fieldQueryRepository;

		public DestinationFieldsValidator(IFieldQueryRepository fieldQueryRepository, IAPILog logger)
		{
			_logger = logger.ForContext<DestinationFieldsValidator>();
			_fieldQueryRepository = fieldQueryRepository;
		}

		public void ValidateDestinationFields(FieldMap[] mappedFields)
		{
			IDictionary<int, string> targetFields = RetrieveTargetFields(_fieldQueryRepository);

			IList<string> missingFields = new List<string>();
			foreach (FieldMap mappedField in mappedFields)
			{
				AddFieldToListIfIsMissingInDestinationWorkspace(targetFields, missingFields, mappedField.DestinationField);
			}

			if (missingFields.Any())
			{
				LogInvalidFieldMappingError(missingFields);
				throw new IntegrationPointsException("Job failed. Destination fields mapped may no longer be available or have been renamed. Please validate your field mapping settings.");
			}
		}

		private static IDictionary<int, string> RetrieveTargetFields(IFieldQueryRepository fieldQueryRepository)
		{
			return fieldQueryRepository
				.RetrieveFieldsAsync(
					(int)Relativity.Client.ArtifactType.Document,
					new HashSet<string>(new string[0]))
				.GetResultsWithoutContextSync()
				.ToDictionary(k => k.ArtifactId, v => v.TextIdentifier);
		}

		private void AddFieldToListIfIsMissingInDestinationWorkspace(IDictionary<int, string> targetFields, IList<string> missingFields, FieldEntry fieldEntry)
		{
			if (string.IsNullOrEmpty(fieldEntry?.FieldIdentifier))
			{
				return;
			}

			int artifactId;
			if (int.TryParse(fieldEntry.FieldIdentifier, out artifactId))
			{
				string fieldName;
				if (!targetFields.TryGetValue(artifactId, out fieldName)
					|| string.Equals(fieldEntry.ActualName, fieldName, StringComparison.OrdinalIgnoreCase))
				{
					missingFields.Add(fieldEntry.ActualName);
				}
			}
		}

		#region Logging
		protected virtual void LogInvalidFieldMappingError(IEnumerable<string> missingFields)
		{
			_logger.LogError("Job failed. Fields mapped may no longer be available or have been renamed. Please validate your field mapping settings. Missing Fields: {@missingFields}", missingFields);
		}
		#endregion
	}
}
