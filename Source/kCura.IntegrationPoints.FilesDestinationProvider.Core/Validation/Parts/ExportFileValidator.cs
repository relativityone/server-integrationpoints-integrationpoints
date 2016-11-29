﻿using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation.Parts
{
	public class ExportFileValidator : BasePartsValidator<IntegrationPointModel>
	{
		private readonly ISerializer _serializer;
		private readonly IExportSettingsBuilder _exportSettingsBuilder;
		private readonly IExportInitProcessService _exportInitProcessService;
		private readonly IExportFileBuilder _exportFileBuilder;

		public ExportFileValidator(ISerializer serializer, IExportSettingsBuilder exportSettingsBuilder, IExportInitProcessService exportInitProcessService, IExportFileBuilder exportFileBuilder)
		{
			_serializer = serializer;
			_exportSettingsBuilder = exportSettingsBuilder;
			_exportInitProcessService = exportInitProcessService;
			_exportFileBuilder = exportFileBuilder;
		}

		public override ValidationResult Validate(IntegrationPointModel value)
		{
			var result = new ValidationResult();

			var exportSettingsEx = _serializer.Deserialize<ExportUsingSavedSearchSettings>(value.SourceConfiguration);
			var destinationSettingsEx = _serializer.Deserialize<Destination>(value.Destination);

			var totalDocCount = _exportInitProcessService.CalculateDocumentCountToTransfer(exportSettingsEx, destinationSettingsEx.ArtifactTypeID);

			var fileCountValidator = new FileCountValidator();
			result.Add(fileCountValidator.Validate(totalDocCount));
			
			var fieldMap = _serializer.Deserialize<IEnumerable<FieldMap>>(value.Map);

			var exportSettings = _exportSettingsBuilder.Create(exportSettingsEx, fieldMap, destinationSettingsEx.ArtifactTypeID);
			var exportFile = _exportFileBuilder.Create(exportSettings);

			var paddingValidator = new PaddingValidator();
			result.Add(paddingValidator.Validate(exportFile, totalDocCount));

			return result;
		}

		private class Destination
		{
			public int ArtifactTypeID { get; set; }
		}
	}
}