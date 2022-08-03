using kCura.IntegrationPoints.Config;
using kCura.WinEDDS;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
    public class LoadFileExportConfig : ConfigBase, IExportConfig
    {
        private readonly IWebApiConfig _webApiConfig;

        private const int _DEF_EXPORT_BATCH_SIZE = 1000;
        private const bool _DEF_TAPI_FORCE_HTTP_CLIENT = false;

        private const int _EXPORT_LOADFILE_ERROR_RETRIES_NUMBER = 2;
        private const int _EXPORT_LOADFILE_ERROR_WAIT_TIME = 10;
        private const int _EXPORT_LOADFILE_IO_ERROR_RETRIES_NUMBER = 2;
        
        private const int _EXPORT_LOADFILE_IO_ERROR_WAIT_TIME = 20;
        
        private const string _EXPORT_BATCH_SIZE_SETTING_NAME = "ExportBatchSize";
        private const string _EXPORT_LOADFILE_ERROR_RETRIES_NUMBER_NAME = "Export.LoadFile.ErrorNumberOfRetries";
        private const string _EXPORT_LOADFILE_ERROR_WAIT_TIME_NAME = "Export.LoadFile.ErrorWaitTime";
        private const string _EXPORT_LOADFILE_IO_ERROR_RETRIES_NUMBER_NAME = "Export.LoadFile.IOErrorNumberOfRetries";

        private const string _EXPORT_LOADFILE_IO_ERROR_WAIT_TIME_NAME = "Export.LoadFile.IOErrorWaitTime";
        private const string _TAPI_FORCE_HTTP_CLIENT = "TapiForceHttpClient";

        public LoadFileExportConfig(IWebApiConfig webApiConfig)
        {
            _webApiConfig = webApiConfig;
        }

        public int ExportBatchSize => GetValue(_EXPORT_BATCH_SIZE_SETTING_NAME, _DEF_EXPORT_BATCH_SIZE);

        public int ExportIOErrorWaitTime => GetValue(_EXPORT_LOADFILE_IO_ERROR_WAIT_TIME_NAME, _EXPORT_LOADFILE_IO_ERROR_WAIT_TIME);

        public int ExportIOErrorNumberOfRetries => GetValue(_EXPORT_LOADFILE_IO_ERROR_RETRIES_NUMBER_NAME, _EXPORT_LOADFILE_IO_ERROR_RETRIES_NUMBER);

        public int ExportErrorNumberOfRetries => GetValue(_EXPORT_LOADFILE_ERROR_RETRIES_NUMBER_NAME, _EXPORT_LOADFILE_ERROR_RETRIES_NUMBER);

        public int ExportErrorWaitTime => GetValue(_EXPORT_LOADFILE_ERROR_WAIT_TIME_NAME, _EXPORT_LOADFILE_ERROR_WAIT_TIME);

        public bool TapiForceHttpClient => GetValue(_TAPI_FORCE_HTTP_CLIENT, _DEF_TAPI_FORCE_HTTP_CLIENT );

        public int ExportLongTextDataGridThreadCount { get; } = 4;

        public bool ExportLongTextObjectManagerEnabled { get; } = true;

        public int ExportLongTextSqlThreadCount { get; } = 2;

        public int HttpErrorNumberOfRetries { get; } = 20;

        public int HttpErrorWaitTimeInSeconds { get; } = 10;

        public string WebApiServiceUrl => _webApiConfig.GetWebApiUrl;

    }
}
