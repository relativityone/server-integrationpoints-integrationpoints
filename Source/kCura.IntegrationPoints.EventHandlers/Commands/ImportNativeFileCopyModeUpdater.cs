using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
    public class ImportNativeFileCopyModeUpdater
    {

        private readonly IProviderTypeService _providerTypeService;

        private static readonly Dictionary<ProviderType, UpdateValues> _updateConditions =
            new Dictionary<ProviderType, UpdateValues>
            {
                {
                    ProviderType.Relativity,
                    new UpdateValues(ImportNativeFileCopyModeEnum.CopyFiles, ImportNativeFileCopyModeEnum.SetFileLinks)
                },
                {
                    ProviderType.ImportLoadFile,
                    new UpdateValues(ImportNativeFileCopyModeEnum.CopyFiles, ImportNativeFileCopyModeEnum.DoNotImportNativeFiles)
                },
                {
                    ProviderType.LDAP,
                    new UpdateValues(ImportNativeFileCopyModeEnum.CopyFiles, ImportNativeFileCopyModeEnum.DoNotImportNativeFiles)
                },
                {
                    ProviderType.FTP,
                    new UpdateValues(ImportNativeFileCopyModeEnum.CopyFiles, ImportNativeFileCopyModeEnum.DoNotImportNativeFiles)
                }
            };

        public const string IMPORT_NATIVE_FILE_COPY_MODE_NODE_NAME = "ImportNativeFileCopyMode";
        public const string IMPORT_NATIVE_FILE_NODE_NAME = "ImportNativeFile";

        public ImportNativeFileCopyModeUpdater(IProviderTypeService providerTypeService)
        {
            _providerTypeService = providerTypeService;
        }

        public string GetCorrectedConfiguration(int? sourceProviderId, int? destinationProviderId, string configuration)
        {
            if (!EnsureInputParametersNotNull(sourceProviderId, destinationProviderId, configuration))
            {
                return null;
            }
            ProviderType providerType =
                _providerTypeService.GetProviderType(sourceProviderId.Value, destinationProviderId.Value);

            string result = null;

            UpdateValues updateValues;
            if (_updateConditions.TryGetValue(providerType, out updateValues))
            {
                result = TryUpdate(configuration, updateValues);
            }

            return result;
        }

        private string TryUpdate(string configurationJson, UpdateValues updateValues)
        {
            string result = null;
            try
            {
                JObject configuration = JObject.Parse(configurationJson);
                JToken importNativeFileNode = configuration.GetValue(IMPORT_NATIVE_FILE_NODE_NAME, StringComparison.InvariantCultureIgnoreCase);
                if (importNativeFileNode != null)
                {
                    configuration[IMPORT_NATIVE_FILE_COPY_MODE_NODE_NAME] = new JValue(importNativeFileNode.Value<bool>() ? updateValues.ValueIfTrue : updateValues.ValueIfFalse);
                    result = configuration.ToString(Formatting.None);
                }
            }
            catch (Exception)
            {
                //Ignore if SourceConfigureation object isn't proper json
            }
            return result;
        }

        private static bool EnsureInputParametersNotNull(int? sourceProviderId, int? destinationProviderId, string sourceConfiguration)
        {
            return sourceConfiguration != null && sourceProviderId != null && destinationProviderId != null;
        }

        private class UpdateValues
        {
            public ImportNativeFileCopyModeEnum ValueIfTrue { get; }
            public ImportNativeFileCopyModeEnum ValueIfFalse { get; }

            public UpdateValues(ImportNativeFileCopyModeEnum valueIfTrue, ImportNativeFileCopyModeEnum valueIfFalse)
            {
                ValueIfTrue = valueIfTrue;
                ValueIfFalse = valueIfFalse;
            }
        }
    }
}