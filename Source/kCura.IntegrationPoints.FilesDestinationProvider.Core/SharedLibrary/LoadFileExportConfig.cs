using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Core.Toggles;
using kCura.WinEDDS;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
	public class LoadFileExportConfig : ConfigBase, IExportConfig
	{
		private bool? _useOldExport;
		private const bool _DEF_FORCE_PARALLELISM_IN_NEW_EXPORT = true;
		private const int _DEF_EXPORT_BATCH_SIZE = 1000;
		private const int _DEF_EXPORT_THREAD_COUNT = 4;
		private const int _DEF_TAPI_BRIDGE_EXPORT_TRANSFER_WAITING_TIME_IN_SECONDS = 600;
		private const bool _DEF_TAPI_FORCE_HTTP_CLIENT = false;
		private const int _DEF_MAXIMUM_FILES_FOR_TAPI_BRIDGE = 10000;

		private const int _EXPORT_LOADFILE_ERROR_RETRIES_NUMBER = 2;
		private const int _EXPORT_LOADFILE_ERROR_WAIT_TIME = 10;
		private const int _EXPORT_LOADFILE_IO_ERROR_RETRIES_NUMBER = 2;
		

		private const int _EXPORT_LOADFILE_IO_ERROR_WAIT_TIME = 20;
		private const int _EXPORT_MAX_NUMBER_OF_TASKS = 2;
		
		private const string _EXPORT_BATCH_SIZE_SETTING_NAME = "ExportBatchSize";
		private const string _EXPORT_LOADFILE_ERROR_RETRIES_NUMBER_NAME = "Export.LoadFile.ErrorNumberOfRetries";
		private const string _EXPORT_LOADFILE_ERROR_WAIT_TIME_NAME = "Export.LoadFile.ErrorWaitTime";
		private const string _EXPORT_LOADFILE_IO_ERROR_RETRIES_NUMBER_NAME = "Export.LoadFile.IOErrorNumberOfRetries";

		private const string _EXPORT_LOADFILE_IO_ERROR_WAIT_TIME_NAME = "Export.LoadFile.IOErrorWaitTime";
		private const string _EXPORT_MAX_NUMBER_OF_TASKS_SETTING_NAME = "MaxNumberOfFileExportTasks";
		private const string _EXPORT_THREAD_COUNT_SETTING_NAME = "ExportThreadCount";
		private const string _FORCE_PARALLELISM_IN_NEW_EXPORT = "Export.ForceParallelismInNewExport";
		private const string _TAPI_BRIDGE_EXPORT_TRANSFER_WAITING_TIME_IN_SECONDS = "TapiBridgeExportTransferWaitingTimeInSeconds";
		private const string _TAPI_FORCE_HTTP_CLIENT = "TapiForceHttpClient";
		private const string _MAXIMUM_FILES_FOR_TAPI_BRIDGE = "MaximumFilesForTapiBridge";


		private readonly IToggleProvider _toggleProvider;

		public bool UseOldExport => (_useOldExport ?? (_useOldExport = _toggleProvider.IsEnabled<UseOldExportVolumeManagerToggle>())).Value;

		public int ExportBatchSize => GetValue(_EXPORT_BATCH_SIZE_SETTING_NAME, _DEF_EXPORT_BATCH_SIZE);

		public int ExportThreadCount => GetValue(_EXPORT_THREAD_COUNT_SETTING_NAME, _DEF_EXPORT_THREAD_COUNT);

		public int ExportIOErrorWaitTime => GetValue(_EXPORT_LOADFILE_IO_ERROR_WAIT_TIME_NAME, _EXPORT_LOADFILE_IO_ERROR_WAIT_TIME);

		public int ExportIOErrorNumberOfRetries => GetValue(_EXPORT_LOADFILE_IO_ERROR_RETRIES_NUMBER_NAME, _EXPORT_LOADFILE_IO_ERROR_RETRIES_NUMBER);

		public int ExportErrorNumberOfRetries => GetValue(_EXPORT_LOADFILE_ERROR_RETRIES_NUMBER_NAME, _EXPORT_LOADFILE_ERROR_RETRIES_NUMBER);

		public int ExportErrorWaitTime => GetValue(_EXPORT_LOADFILE_ERROR_WAIT_TIME_NAME, _EXPORT_LOADFILE_ERROR_WAIT_TIME);

		public int MaxNumberOfFileExportTasks => GetValue(_EXPORT_MAX_NUMBER_OF_TASKS_SETTING_NAME, _EXPORT_MAX_NUMBER_OF_TASKS);

		public int TapiBridgeExportTransferWaitingTimeInSeconds => GetValue(_TAPI_BRIDGE_EXPORT_TRANSFER_WAITING_TIME_IN_SECONDS, _DEF_TAPI_BRIDGE_EXPORT_TRANSFER_WAITING_TIME_IN_SECONDS);
		public bool TapiForceHttpClient => GetValue(_TAPI_FORCE_HTTP_CLIENT, _DEF_TAPI_FORCE_HTTP_CLIENT );
		public int MaximumFilesForTapiBridge => GetValue(_MAXIMUM_FILES_FOR_TAPI_BRIDGE, _DEF_MAXIMUM_FILES_FOR_TAPI_BRIDGE);

		public bool ForceParallelismInNewExport => GetValue(_FORCE_PARALLELISM_IN_NEW_EXPORT, _DEF_FORCE_PARALLELISM_IN_NEW_EXPORT);

		public LoadFileExportConfig(IToggleProvider toggleProvider)
		{
			_toggleProvider = toggleProvider;
			
		}
	}
}
