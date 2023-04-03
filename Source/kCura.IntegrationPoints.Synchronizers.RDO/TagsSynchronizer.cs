using System;
using System.Collections.Generic;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Readers;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
    public class TagsSynchronizer : IDataSynchronizer
    {
        private readonly ISerializer _serializer;
        private readonly IDataSynchronizer _rdoSynchronizer;
        private readonly IAPILog _logger;

        public TagsSynchronizer(IHelper helper, IDataSynchronizer rdoSynchronizer, ISerializer serializer)
        {
            _rdoSynchronizer = rdoSynchronizer;
            _serializer = serializer;
            _logger = helper.GetLoggerFactory().GetLogger().ForContext<TagsSynchronizer>();
        }

        public int TotalRowsProcessed => _rdoSynchronizer.TotalRowsProcessed;

        public IEnumerable<FieldEntry> GetFields(DataSourceProviderConfiguration providerConfiguration)
        {
            var destinationConfiguration = _serializer.Deserialize<DestinationConfiguration>(providerConfiguration.Configuration);
            UpdateImportSettingsForTagging(destinationConfiguration);
            providerConfiguration.Configuration = _serializer.Serialize(destinationConfiguration);
            return _rdoSynchronizer.GetFields(providerConfiguration);
        }

        public void SyncData(
            IEnumerable<IDictionary<FieldEntry, object>> data,
            IEnumerable<FieldMap> fieldMap,
            ImportSettings options,
            IJobStopManager jobStopManager,
            IDiagnosticLog diagnosticLog)
        {
            try
            {
                UpdateImportSettingsForTagging(options.DestinationConfiguration);
                _rdoSynchronizer.SyncData(data, fieldMap, options, jobStopManager, diagnosticLog);
            }
            catch (Exception ex)
            {
                LogAndThrowSyncDataException(ex);
            }
        }

        public void SyncData(
            IDataTransferContext data,
            IEnumerable<FieldMap> fieldMap,
            ImportSettings options,
            IJobStopManager jobStopManager,
            IDiagnosticLog diagnosticLog)
        {
            try
            {
                UpdateImportSettingsForTagging(options.DestinationConfiguration);
                _rdoSynchronizer.SyncData(data, fieldMap, options, jobStopManager, diagnosticLog);
            }
            catch (Exception ex)
            {
                LogAndThrowSyncDataException(ex);
            }
        }

        private void LogAndThrowSyncDataException(Exception exception)
        {
            string message = @"Error occured while syncing tags";
            _logger.LogError(message);
            throw new IntegrationPointsException(message, exception) { ShouldAddToErrorsTab = true };
        }

        private static void UpdateImportSettingsForTagging(DestinationConfiguration destinationConfiguration)
        {
            destinationConfiguration.ProductionImport = false;
            destinationConfiguration.ImageImport = false;
            destinationConfiguration.UseDynamicFolderPath = false;
            destinationConfiguration.ImportNativeFileCopyMode = ImportNativeFileCopyModeEnum.DoNotImportNativeFiles;
        }
    }
}
