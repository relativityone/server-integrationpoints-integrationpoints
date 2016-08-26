using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Models;
using kCura.ScheduleQueue.Core;

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

		public void StartWith(ExportSettings settings, Job job)
		{
			var exporter = _exportProcessBuilder.Create(settings, job);
			exporter.Run();
		}

		public void StartWith(ExportUsingSavedSearchSettings sourceSettings, IEnumerable<FieldMap> fieldMap, int artifactTypeId, Job job)
		{
			var exportSettings = _exportSettingsBuilder.Create(sourceSettings, fieldMap, artifactTypeId);
			StartWith(exportSettings, job);
		}
	}
}