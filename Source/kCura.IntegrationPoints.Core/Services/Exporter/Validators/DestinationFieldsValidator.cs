using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Exceptions;
using Relativity;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;

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

            IList<int> missingFields = new List<int>();
            foreach (FieldMap mappedField in mappedFields)
            {
                AddFieldToListIfIsMissingInDestinationWorkspace(targetFields, missingFields, mappedField.DestinationField);
            }

            if (missingFields.Any())
            {
                LogInvalidFieldMappingError(missingFields);
                throw new IntegrationPointsException("Job failed. Destination fields mapped may no longer be available. Please validate your field mapping settings.");
            }
        }

        private static IDictionary<int, string> RetrieveTargetFields(IFieldQueryRepository fieldQueryRepository)
        {
            return fieldQueryRepository
                .RetrieveFieldsAsync(
                    (int)ArtifactType.Document,
                    new HashSet<string>(new string[0]))
                .GetAwaiter().GetResult()
                .ToDictionary(k => k.ArtifactId, v => v.TextIdentifier);
        }

        private void AddFieldToListIfIsMissingInDestinationWorkspace(IDictionary<int, string> targetFields, IList<int> missingFields, FieldEntry fieldEntry)
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
                    missingFields.Add(artifactId);
                }
            }
        }

        #region Logging
        protected virtual void LogInvalidFieldMappingError(IEnumerable<int> missingFields)
        {
            _logger.LogError("Job failed. Fields mapped may no longer be available. Please validate your field mapping settings. Missing Fields: {@missingFields}", missingFields);
        }
        #endregion
    }
}
