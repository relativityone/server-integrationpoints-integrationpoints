﻿using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation.Parts
{
	public class FieldsMapValidator : BasePartsValidator<IntegrationPointProviderValidationModel>
	{
		private readonly IAPILog _logger;
		private readonly ISerializer _serializer;
		private readonly IExportFieldsService _exportFieldsService;

		public FieldsMapValidator(IAPILog logger, ISerializer serializer, IExportFieldsService exportFieldsService)
		{
			_logger = logger.ForContext<FieldsMapValidator>();
			_serializer = serializer;
			_exportFieldsService = exportFieldsService;
		}

		public override ValidationResult Validate(IntegrationPointProviderValidationModel value)
		{
			var result = new ValidationResult();

			if (string.IsNullOrWhiteSpace(value.FieldsMap))
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

				var exportableFields = RetrieveExportableFields(value, exportSettings);
				var selectedFields = fieldMap.Select(x => x.SourceField);

				var orphanedFields = selectedFields.Except(exportableFields, new FieldEntryEqualityComparer());

				foreach (var field in orphanedFields)
				{
					result.Add($"{field.DisplayName ?? "<empty>"} {FileDestinationProviderValidationMessages.FIELD_MAP_UNKNOWN_FIELD}");
				}
			}

			return result;
		}

		private FieldEntry[] RetrieveExportableFields(IntegrationPointProviderValidationModel value,
			ExportUsingSavedSearchSettings exportSettings)
		{
			try
			{
				return _exportFieldsService.GetAllExportableFields(exportSettings.SourceWorkspaceArtifactId, value.ArtifactTypeId);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occured while retriving exportable fields in {validator}", nameof(FieldsMapValidator));
				throw new IntegrationPointsException($"An error occured while retriving exportable fields in {nameof(FieldsMapValidator)}", ex);
			}
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