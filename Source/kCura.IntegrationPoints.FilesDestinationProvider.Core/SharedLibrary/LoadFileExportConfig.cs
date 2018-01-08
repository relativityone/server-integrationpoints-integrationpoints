using kCura.IntegrationPoints.Config;
using kCura.WinEDDS;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
	public class LoadFileExportConfig : ConfigBase, IExportConfig
	{
		private ExportConfig _exportConfig = new ExportConfig();

		private const string _EXPORT_BATCH_SIZE_SETTING_NAME = "ExportBatchSize";
		private const string _EXPORT_THREAD_COUNT_SETTING_NAME = "ExportThreadCount";

		private const int _DEF_EXPORT_BATCH_SIZE = 1000;
		private const int _DEF_EXPORT_THREAD_COUNT = 4;

		public int ExportBatchSize => GetValue(_EXPORT_BATCH_SIZE_SETTING_NAME, _DEF_EXPORT_BATCH_SIZE);

		public int ExportThreadCount => GetValue(_EXPORT_THREAD_COUNT_SETTING_NAME, _DEF_EXPORT_THREAD_COUNT);

		public int ExportIOErrorWaitTime => _exportConfig.ExportIOErrorWaitTime;
		public int ExportIOErrorNumberOfRetries => _exportConfig.ExportIOErrorNumberOfRetries;
		public int ExportErrorNumberOfRetries => _exportConfig.ExportErrorNumberOfRetries;
		public int ExportErrorWaitTime => _exportConfig.ExportErrorWaitTime;
	}
}
