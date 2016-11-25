using System;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation.Parts;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation
{
	public interface IValidatorsFactory
	{
		ExportFileValidator CreateExportFileValidator();
	}

	public class ValidatorsFactory : IValidatorsFactory
	{
		private readonly ISerializer _serializer;
		private readonly IExportSettingsBuilder _exportSettingsBuilder;
		private readonly IExportInitProcessService _exportInitProcessService;
		private readonly IExportFileBuilder _exportFileBuilder;

		public ValidatorsFactory(ISerializer serializer, IExportSettingsBuilder exportSettingsBuilder, IExportInitProcessService exportInitProcessService, IExportFileBuilder exportFileBuilder)
		{
			_serializer = serializer;
			_exportSettingsBuilder = exportSettingsBuilder;
			_exportInitProcessService = exportInitProcessService;
			_exportFileBuilder = exportFileBuilder;
		}

		public ExportFileValidator CreateExportFileValidator()
		{
			return new ExportFileValidator(_serializer, _exportSettingsBuilder, _exportInitProcessService, _exportFileBuilder);
		}
	}
}