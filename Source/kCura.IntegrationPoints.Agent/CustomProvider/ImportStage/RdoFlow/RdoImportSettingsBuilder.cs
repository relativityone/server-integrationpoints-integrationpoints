using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.API;
using Relativity.Import.V1.Builders.Rdos;
using Relativity.Import.V1.Models.Settings;

namespace kCura.IntegrationPoints.Agent.CustomProvider.ImportStage.RdoFlow
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
        public RdoImportConfiguration Build(CustomProviderDestinationConfiguration destinationConfiguration, List<IndexedFieldMap> fieldMappings, IndexedFieldMap overlyIdentifierField)
        {
            IWithOverlayMode overlayModeSettings = ImportRdoSettingsBuilder.Create();

            _logger.LogInformation("Indexed FieldsMapping: {@fieldsMapping}", fieldMappings);

            IWithFields fieldsSettings = ConfigureOverwriteModeSettings(
                overlayModeSettings,
                destinationConfiguration.ImportOverwriteMode,
                destinationConfiguration.FieldOverlayBehavior,
                overlyIdentifierField.DestinationFieldName);

            IWithRdo withRdo = ConfigureFieldsMappingSettings(
                fieldsSettings,
                fieldMappings);

            ImportRdoSettings importSettings = ConfigureArtifactType(withRdo, destinationConfiguration);
            _logger.LogInformation("Job Import Settings: {@settings}", importSettings);

            AdvancedImportSettings advancedSettings = CreateAdvancedImportSettings();
            _logger.LogInformation("Job Advanced Import Settings: {@advancedSettings}", advancedSettings);

            return new RdoImportConfiguration(importSettings, advancedSettings);
        }

        private AdvancedImportSettings CreateAdvancedImportSettings()
        {
            var advancedSettings = new AdvancedImportSettings();

            return advancedSettings;
        }

        private IWithFields ConfigureOverwriteModeSettings(
            IWithOverlayMode overlayModeSettings,
            ImportOverwriteModeEnum overwriteMode,
            string overlayBehavior,
            string overlayIdentifierFieldName)
        {
            _logger.LogInformation(
                "Configuring OverlayMode - OverwriteMode: {overwriteMode}, OverlayBehavior: {overlayBehavior}, OverlayIdentifierField: {overlayIdentifierField}",
                overwriteMode,
                overlayBehavior,
                overlayIdentifierFieldName);
            switch (overwriteMode)
            {
                case ImportOverwriteModeEnum.AppendOnly:
                    return overlayModeSettings.WithAppendMode();

                case ImportOverwriteModeEnum.AppendOverlay:
                    return overlayModeSettings.WithAppendOverlayMode(
                        x => x.WithKeyField(overlayIdentifierFieldName)
                            .WithMultiFieldOverlayBehaviour(ToMultiFieldOverlayBehaviour(overlayBehavior)));

                case ImportOverwriteModeEnum.OverlayOnly:
                    return overlayModeSettings.WithOverlayMode(
                        x => x.WithKeyField(overlayIdentifierFieldName)
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
            CustomProviderDestinationConfiguration destinationConfiguration)
        {
            return withRdo.WithRdo(f => f
                .WithArtifactTypeId(destinationConfiguration.ArtifactTypeId)
                .WithoutParentColumnIndex());
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
