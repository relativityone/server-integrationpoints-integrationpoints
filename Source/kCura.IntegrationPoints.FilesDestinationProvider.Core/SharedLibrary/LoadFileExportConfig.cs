using kCura.IntegrationPoints.Config;
using kCura.WinEDDS;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
	public class LoadFileExportConfig : ConfigBase, IExportConfig
	{
	    private const string _USE_OLD_EXPORT_SETTING_NAME = "UseOldExport";

        private const string _EXPORT_BATCH_SIZE_SETTING_NAME = "ExportBatchSize";
		private const string _EXPORT_THREAD_COUNT_SETTING_NAME = "ExportThreadCount";

		private const string _EXPORT_LOADFILE_IO_ERROR_WAIT_TIME_NAME = "Export.LoadFile.IOErrorWaitTime";
		private const string _EXPORT_LOADFILE_IO_ERROR_RETRIES_NUMBER_NAME = "Export.LoadFile.IOErrorNumberOfRetries";
		private const string _EXPORT_LOADFILE_ERROR_WAIT_TIME_NAME = "Export.LoadFile.ErrorWaitTime";
		private const string _EXPORT_LOADFILE_ERROR_RETRIES_NUMBER_NAME = "Export.LoadFile.ErrorNumberOfRetries";

	    private const bool _DEF_USE_OLD_EXPORT = false;

        private const int _DEF_EXPORT_BATCH_SIZE = 1000;
		private const int _DEF_EXPORT_THREAD_COUNT = 4;

		private const int _EXPORT_LOADFILE_IO_ERROR_WAIT_TIME = 20;
		private const int _EXPORT_LOADFILE_IO_ERROR_RETRIES_NUMBER = 2;
		private const int _EXPORT_LOADFILE_ERROR_WAIT_TIME = 10;
		private const int _EXPORT_LOADFILE_ERROR_RETRIES_NUMBER = 2;
        
	    public bool UseOldExport => GetValue(_USE_OLD_EXPORT_SETTING_NAME, _DEF_USE_OLD_EXPORT);

        public int ExportBatchSize => GetValue(_EXPORT_BATCH_SIZE_SETTING_NAME, _DEF_EXPORT_BATCH_SIZE);

		public int ExportThreadCount => GetValue(_EXPORT_THREAD_COUNT_SETTING_NAME, _DEF_EXPORT_THREAD_COUNT);

	    public int ExportIOErrorWaitTime => GetValue(_EXPORT_LOADFILE_IO_ERROR_WAIT_TIME_NAME, _EXPORT_LOADFILE_IO_ERROR_WAIT_TIME);

		public int ExportIOErrorNumberOfRetries => GetValue(_EXPORT_LOADFILE_IO_ERROR_RETRIES_NUMBER_NAME, _EXPORT_LOADFILE_IO_ERROR_RETRIES_NUMBER);

		public int ExportErrorNumberOfRetries => GetValue(_EXPORT_LOADFILE_ERROR_RETRIES_NUMBER_NAME, _EXPORT_LOADFILE_ERROR_RETRIES_NUMBER);

		public int ExportErrorWaitTime => GetValue(_EXPORT_LOADFILE_ERROR_WAIT_TIME_NAME, _EXPORT_LOADFILE_ERROR_WAIT_TIME);
	}
}
