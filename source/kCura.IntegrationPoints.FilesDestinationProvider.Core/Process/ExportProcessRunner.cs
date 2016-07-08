using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Domain.Models;
using kCura.WinEDDS;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Process
{
	public class ExportProcessRunner
	{
		private readonly IExportProcessBuilder _exportProcessBuilder;
		private readonly IExportSettingsBuilder _exportSettingsBuilder;

		public ExportProcessRunner(IExportProcessBuilder exportProcessBuilder, IExportSettingsBuilder exportSettingsBuilder)
		{
			_exportProcessBuilder = exportProcessBuilder;
			_exportSettingsBuilder = exportSettingsBuilder;
		}

		public void StartWith(ExportSettings settings)
		{
			var exporter = _exportProcessBuilder.Create(settings);
			exporter.Run();
		}

		public void StartWith(ExportUsingSavedSearchSettings sourceSettings, IEnumerable<FieldMap> fieldMap, int artifactTypeId)
		{
			var exportSettings = _exportSettingsBuilder.Create(sourceSettings, fieldMap, artifactTypeId);
			StartWith(exportSettings);
		}
	}
}