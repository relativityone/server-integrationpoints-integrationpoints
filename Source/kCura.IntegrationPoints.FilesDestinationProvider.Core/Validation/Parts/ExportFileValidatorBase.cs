﻿using System.Collections.Generic;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using kCura.WinEDDS;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation.Parts
{
	public abstract class ExportFileValidatorBase : BasePartsValidator<IntegrationPointModel>
	{
		private readonly ISerializer _serializer;
		private readonly IExportSettingsBuilder _exportSettingsBuilder;
		private readonly IExportFileBuilder _exportFileBuilder;

		protected ExportUsingSavedSearchSettings ExportSettingsEx { get; private set; }
		protected DestinationConfiguration DestinationSettingsEx { get; private set; }

		protected ExportFileValidatorBase(ISerializer serializer, IExportSettingsBuilder exportSettingsBuilder, IExportFileBuilder exportFileBuilder)
		{
			_serializer = serializer;
			_exportSettingsBuilder = exportSettingsBuilder;
			_exportFileBuilder = exportFileBuilder;
		}

		protected abstract ValidationResult PerformValidation(ExportFile exportFile);

		public override ValidationResult Validate(IntegrationPointModel value)
		{
			var exportFile = PrepareExportFileForValidation(value);

			return PerformValidation(exportFile);
		}

		protected ExportFile PrepareExportFileForValidation(IntegrationPointModel value)
		{
			ExportSettingsEx = _serializer.Deserialize<ExportUsingSavedSearchSettings>(value.SourceConfiguration);
			DestinationSettingsEx = _serializer.Deserialize<DestinationConfiguration>(value.Destination);

			var fieldMap = _serializer.Deserialize<IEnumerable<FieldMap>>(value.Map);

			var exportSettings = _exportSettingsBuilder.Create(ExportSettingsEx, fieldMap, DestinationSettingsEx.ArtifactTypeId);
			return _exportFileBuilder.Create(exportSettings);
		}


	}
}
