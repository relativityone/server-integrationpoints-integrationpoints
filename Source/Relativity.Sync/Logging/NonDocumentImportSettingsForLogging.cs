using kCura.Relativity.DataReaderClient;

namespace Relativity.Sync.Logging
{
    internal class NonDocumentImportSettingsForLogging : ImportSettingsForLoggingBase
    {
        private NonDocumentImportSettingsForLogging(ImportSettingsBase settings) : base(settings)
        {
        }

        public static NonDocumentImportSettingsForLogging CreateWithoutSensitiveData(ImportSettingsBase settings)
        {
            return new NonDocumentImportSettingsForLogging(settings);
        }
    }
}
