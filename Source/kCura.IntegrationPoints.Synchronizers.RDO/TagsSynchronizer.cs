using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.IntegrationPoints.Domain.Synchronizer;
using Newtonsoft.Json;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
    public class TagsSynchronizer : IDataSynchronizer
    {
        private readonly IDataSynchronizer _rdoSynchronizer;
        private readonly IAPILog _logger;
        public TagsSynchronizer(IHelper helper, IDataSynchronizer rdoSynchronizer)
        {
            _rdoSynchronizer = rdoSynchronizer;
            _logger = helper.GetLoggerFactory().GetLogger().ForContext<TagsSynchronizer>();
        }

        public IEnumerable<FieldEntry> GetFields(DataSourceProviderConfiguration providerConfiguration)
        {
            providerConfiguration.Configuration = UpdateImportSettingsForTagging(providerConfiguration.Configuration);
            return _rdoSynchronizer.GetFields(providerConfiguration);
        }

        public void SyncData(IEnumerable<IDictionary<FieldEntry, object>> data, IEnumerable<FieldMap> fieldMap,
            string options, IJobStopManager jobStopManager)
        {
            try
            {
                string updatedOptions = UpdateImportSettingsForTagging(options);
                _rdoSynchronizer.SyncData(data, fieldMap, updatedOptions, jobStopManager);
            }
            catch (System.Exception ex)
            {
                LogAndThrowSyncDataException(ex);
            }
        }

        public void SyncData(IDataTransferContext data, IEnumerable<FieldMap> fieldMap, string options,
            IJobStopManager jobStopManager)
        {
            try { 
                string updatedOptions = UpdateImportSettingsForTagging(options);
                _rdoSynchronizer.SyncData(data, fieldMap, updatedOptions, jobStopManager);
            }
            catch (System.Exception ex)
            {
                LogAndThrowSyncDataException(ex);
            }
        }

        public int TotalRowsProcessed => _rdoSynchronizer.TotalRowsProcessed;

        private string UpdateImportSettingsForTagging(string currentOptions)
        {
            ImportSettings importSettings = JsonConvert.DeserializeObject<ImportSettings>(currentOptions);
            importSettings.ProductionImport = false;
            importSettings.ImageImport = false;
            importSettings.UseDynamicFolderPath = false;
            importSettings.ImportNativeFileCopyMode = ImportNativeFileCopyModeEnum.DoNotImportNativeFiles;
            return JsonConvert.SerializeObject(importSettings);
        }

        private void LogAndThrowSyncDataException(Exception exception)
        {
            string message = @"Error occured while syncing tags";
            _logger.LogError(message);
            throw new IntegrationPointsException(message, exception) { ShouldAddToErrorsTab = true };
        }
    }
}