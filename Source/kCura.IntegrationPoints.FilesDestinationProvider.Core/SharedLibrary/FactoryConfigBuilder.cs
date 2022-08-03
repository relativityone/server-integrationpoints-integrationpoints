using System.Collections.Generic;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Extensions;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers.FileNaming;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using kCura.WinEDDS;
using kCura.WinEDDS.Service.Export;
using Relativity.DataExchange.Export;
using Relativity.DataExchange.Export.Natives.Name.Factories;
using Relativity.DataExchange.Process;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
    internal class FactoryConfigBuilder : IFactoryConfigBuilder
    {
        private readonly JobHistoryErrorServiceProvider _jobHistoryErrorServiceProvider;
        private readonly IFileNameProvidersDictionaryBuilder _fileNameProvidersDictionaryBuilder;
        private readonly IExportConfig _exportConfig;

        public FactoryConfigBuilder(JobHistoryErrorServiceProvider jobHistoryErrorServiceProvider, IFileNameProvidersDictionaryBuilder fileNameProvidersDictionaryBuilder, IExportConfig exportConfig)
        {
            _jobHistoryErrorServiceProvider = jobHistoryErrorServiceProvider;
            _fileNameProvidersDictionaryBuilder = fileNameProvidersDictionaryBuilder;
            _exportConfig = exportConfig;
        }

        public ExporterFactoryConfig BuildFactoryConfig(ExportDataContext exportDataContext, IServiceFactory serviceFactory)
        {
            var config = new ExporterFactoryConfig
            {
                JobStopManager = _jobHistoryErrorServiceProvider?.JobHistoryErrorService.JobStopManager,
                Controller = new ProcessContext(),
                ServiceFactory = serviceFactory,
                LoadFileFormatterFactory = new ExportFileFormatterFactory(new ExtendedFieldNameProvider(exportDataContext.Settings)),
                NameTextAndNativesAfterBegBates = exportDataContext.ExportFile.AreSettingsApplicableForProdBegBatesNameCheck(),
                ExportConfig = _exportConfig
            };

            IDictionary<ExportNativeWithFilenameFrom, IFileNameProvider> fileNameProvidersDictionary = _fileNameProvidersDictionaryBuilder.Build(exportDataContext);

            var fileNameProviderContainerFactory = new FileNameProviderContainerFactory(fileNameProvidersDictionary);
            config.FileNameProvider = fileNameProviderContainerFactory.Create(exportDataContext.ExportFile);
            return config;
        }
    }
}