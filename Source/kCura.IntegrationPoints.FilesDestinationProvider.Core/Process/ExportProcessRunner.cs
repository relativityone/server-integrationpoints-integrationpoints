using System.Collections.Generic;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.ScheduleQueue.Core;
using Relativity.API;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Process
{
	public class ExportProcessRunner
	{
		private readonly IExportProcessBuilder _exportProcessBuilder;
		private readonly IExportSettingsBuilder _exportSettingsBuilder;
		private readonly IAPILog _logger;

		public ExportProcessRunner(IExportProcessBuilder exportProcessBuilder, IExportSettingsBuilder exportSettingsBuilder, IHelper helper)
		{
			_exportProcessBuilder = exportProcessBuilder;
			_exportSettingsBuilder = exportSettingsBuilder;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<ExportProcessBuilder>();
		}

		public void StartWith(ExportSettings settings, Job job)
		{
			LogStartingExport();
			IExporter exporter = _exportProcessBuilder.Create(settings, job);
			exporter.ExportSearch();
			LogFinishingExport();
		}

		public void StartWith(ExportUsingSavedSearchSettings sourceSettings, IEnumerable<FieldMap> fieldMap, int artifactTypeId, Job job)
		{
			var exportSettings = _exportSettingsBuilder.Create(sourceSettings, fieldMap, artifactTypeId);
			StartWith(exportSettings, job);
		}

		#region Logging

		private void LogStartingExport()
		{
			_logger.LogInformation("Starting Export.");
		}

		private void LogFinishingExport()
		{
			_logger.LogInformation("Finishing Export.");
		}

		#endregion
	}
}