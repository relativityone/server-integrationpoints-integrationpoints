using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Extensions;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation.Parts
{
	public class FieldsMapValidator : BasePartsValidator<IntegrationPointProviderValidationModel>
	{
		private readonly ISerializer _serializer;
		private readonly IExportFieldsService _exportFieldsService;

		public FieldsMapValidator(ISerializer serializer, IExportFieldsService exportFieldsService)
		{
			_serializer = serializer;
			_exportFieldsService = exportFieldsService;
		}

		public override ValidationResult Validate(IntegrationPointProviderValidationModel value)
		{
			var result = new ValidationResult();

			if (String.IsNullOrWhiteSpace(value.FieldsMap))
			{
				result.Add(FileDestinationProviderValidationMessages.FIELD_MAP_NO_FIELDS);
				return result;
			}

			var fieldMap = _serializer.Deserialize<IEnumerable<FieldMap>>(value.FieldsMap);

			if (fieldMap.Count() == 0)
			{
				result.Add(FileDestinationProviderValidationMessages.FIELD_MAP_NO_FIELDS);
			}
			else
			{
				var exportSettings = _serializer.Deserialize<ExportUsingSavedSearchSettings>(value.SourceConfiguration);

				var exportableFields = _exportFieldsService.GetAllExportableFields(exportSettings.SourceWorkspaceArtifactId, value.ArtifactTypeId);
				var selectedFields = fieldMap.Select(x => x.SourceField);

				var orphanedFields = selectedFields.Except(exportableFields, new FieldsMapValidator.FieldEntryEqualityComparer());

				foreach (var field in orphanedFields)
				{
					result.Add($"{field.DisplayName ?? "<empty>"} {FileDestinationProviderValidationMessages.FIELD_MAP_UNKNOWN_FIELD}");
				}
			}

			return result;
		}

		private class FieldEntryEqualityComparer : IEqualityComparer<FieldEntry>
		{
			public bool Equals(FieldEntry x, FieldEntry y)
			{
				if ((x == null) && (y == null))
				{
					return true;
				}

				if ((x == null) || (y == null))
				{
					return false;
				}

				return (x.FieldIdentifier == y.FieldIdentifier);
			}

			public int GetHashCode(FieldEntry obj)
			{
				return (obj?.FieldIdentifier == null) ? 0 : obj.FieldIdentifier.GetHashCode();
			}
		}
	}
}