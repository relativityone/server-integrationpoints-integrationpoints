using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.API;
using Relativity.Import.V1.Builders.Rdos;
using Relativity.Import.V1.Models.Settings;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services
{
    /// <inheritdoc />
    internal class RdoImportSettingsBuilder : IRdoImportSettingsBuilder
    {
        private readonly IAPILog _logger;

        public RdoImportSettingsBuilder(IAPILog logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public RdoImportConfiguration Build(ImportSettings destinationConfiguration, List<IndexedFieldMap> fieldMappings)
        {
            IWithOverlayMode overlayModeSettings = ImportRdoSettingsBuilder.Create();

            IndexedFieldMap identifier = GetIdentifierField(fieldMappings);

            IWithFields fieldsSettings = ConfigureOverwriteModeSettings(
                overlayModeSettings,
                destinationConfiguration.ImportOverwriteMode,
                destinationConfiguration.FieldOverlayBehavior,
                identifier.DestinationFieldName);

            IWithRdo withRdo = ConfigureFieldsMappingSettings(
                fieldsSettings,
                fieldMappings);

            ImportRdoSettings importSettings = ConfigureArtifactType(withRdo, destinationConfiguration);

            AdvancedImportSettings advancedSettings = CreateAdvancedImportSettings();

            return new RdoImportConfiguration(importSettings, advancedSettings);
        }

        private AdvancedImportSettings CreateAdvancedImportSettings()
        {
            var advancedSettings = new AdvancedImportSettings()
            {
                Other = new AdvancedOtherSettings
                {
                    AuditLevel = AuditLevel.FullAudit,
                    BatchSize = 1000
                }
            };

            return advancedSettings;
        }

        private IWithFields ConfigureOverwriteModeSettings(
            IWithOverlayMode overlayModeSettings,
            ImportOverwriteModeEnum overwriteMode,
            string overlayBehavior,
            string identifierFieldName)
        {
            _logger.LogInformation(
                "Configuring OverlayMode - OverwriteMode: {overwriteMode}, OverlayBehavior: {overlayBehavior}", overwriteMode, overlayBehavior);
            switch (overwriteMode)
            {
                case ImportOverwriteModeEnum.AppendOnly:
                    return overlayModeSettings.WithAppendMode();

                case ImportOverwriteModeEnum.AppendOverlay:
                    return overlayModeSettings.WithAppendOverlayMode(
                        x => x.WithKeyField(identifierFieldName)
                            .WithMultiFieldOverlayBehaviour(ToMultiFieldOverlayBehaviour(overlayBehavior)));

                case ImportOverwriteModeEnum.OverlayOnly:
                    return overlayModeSettings.WithOverlayMode(
                        x => x.WithKeyField(identifierFieldName)
                            .WithMultiFieldOverlayBehaviour(ToMultiFieldOverlayBehaviour(overlayBehavior)));
                default:
                    throw new NotSupportedException($"ImportOverwriteMode {overwriteMode} is not supported.");
            }
        }

        private IWithRdo ConfigureFieldsMappingSettings(IWithFields fieldsSettings, List<IndexedFieldMap> fieldMappings)
        {
            _logger.LogInformation("Configuring FieldsMapping...");
            return fieldsSettings.WithFieldsMapped(x =>
            {
                foreach (IndexedFieldMap map in fieldMappings)
                {
                    _logger.LogInformation("Configure Field - {@field}", map);
                    x = x.WithField(map.ColumnIndex, map.DestinationFieldName);

                    // TODO Consider if we want store Long Texts in separate load files.
                    // TODO If so, we need to request a feature to IAPI Team to add appropriate method (analogous to WithLongTextFieldContainingFilePath for document configuration flow)
                }
            });
        }

        private ImportRdoSettings ConfigureArtifactType(
            IWithRdo withRdo,
            ImportSettings importSettings)
        {
            return withRdo
                .WithRdo(f => f
                    .WithArtifactTypeId(importSettings.ArtifactTypeId)
                    .WithoutParentColumnIndex());
        }

        private static IndexedFieldMap GetIdentifierField(List<IndexedFieldMap> fieldMappings)
        {
            return fieldMappings.FirstOrDefault(x => x.FieldMap.DestinationField.IsIdentifier);
        }

        private static MultiFieldOverlayBehaviour ToMultiFieldOverlayBehaviour(string overlayBehaviorString)
        {
            switch (overlayBehaviorString)
            {
                case ImportSettings.FIELDOVERLAYBEHAVIOR_MERGE: return MultiFieldOverlayBehaviour.MergeAll;
                case ImportSettings.FIELDOVERLAYBEHAVIOR_REPLACE: return MultiFieldOverlayBehaviour.ReplaceAll;
                case ImportSettings.FIELDOVERLAYBEHAVIOR_DEFAULT: return MultiFieldOverlayBehaviour.UseRelativityDefaults;
                default: throw new NotSupportedException($"Unknown {nameof(overlayBehaviorString)} value: {overlayBehaviorString}");
            }
        }
    }
}
