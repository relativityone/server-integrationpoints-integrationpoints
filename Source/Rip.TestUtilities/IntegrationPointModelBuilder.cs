using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.Services.Objects.DataContracts;

namespace Rip.TestUtilities
{
    public class IntegrationPointModelBuilder
    {
        private const string _APPEND_ONLY = "Append Only";
        private const string _OVERLAY_ONLY = "Overlay Only";
        private const string _APPEND_OVERLAY = "Append/Overlay";
        private readonly ISerializer _serializer;
        private readonly IRelativityObjectManager _objectManager;
        private readonly IEnumerable<SourceProvider> _sourceProviders;
        private readonly IEnumerable<DestinationProvider> _destinationProviders;
        private int _type;
        private string _name;
        private int _sourceProvider;
        private int _destinationProvider;
        private string _sourceConfiguration;
        private DestinationConfiguration _destinationConfiguration;
        private List<FieldMap> _fieldMapping;
        private string _overwriteMode = _APPEND_ONLY;

        public IntegrationPointModelBuilder(ISerializer serializer, IRelativityObjectManager objectManager)
        {
            _serializer = serializer;
            _objectManager = objectManager;
            _sourceProviders = GetSourceProviders();
            _destinationProviders = GetDestinationProviders();
        }

        public IntegrationPointModelBuilder WithType(int type)
        {
            _type = type;
            return this;
        }

        public IntegrationPointModelBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public IntegrationPointModelBuilder WithSourceProvider(string sourceProviderName)
        {
            _sourceProvider = _sourceProviders.Single(provider => provider.Name == sourceProviderName).ArtifactId;
            return this;
        }

        public IntegrationPointModelBuilder WithDestinationProvider(string destinationProviderName)
        {
            _destinationProvider = _destinationProviders.Single(provider => provider.Name == destinationProviderName).ArtifactId;
            return this;
        }

        public IntegrationPointModelBuilder WithSourceConfiguration(SourceConfiguration sourceConfiguration)
        {
            _sourceConfiguration = _serializer.Serialize(sourceConfiguration);
            return this;
        }

        public IntegrationPointModelBuilder WithDestinationConfiguration(DestinationConfiguration destinationConfiguration)
        {
            _destinationConfiguration = destinationConfiguration;
            return this;
        }

        public IntegrationPointModelBuilder WithFieldMapping(FieldMap[] fieldMapping)
        {
            _fieldMapping = fieldMapping.ToList();
            return this;
        }

        public IntegrationPointModelBuilder WithOverwriteMode(ImportOverwriteModeEnum overwriteMode)
        {
            switch (overwriteMode)
            {
                case ImportOverwriteModeEnum.AppendOnly:
                    _overwriteMode = _APPEND_ONLY;
                    break;
                case ImportOverwriteModeEnum.OverlayOnly:
                    _overwriteMode = _OVERLAY_ONLY;
                    break;
                case ImportOverwriteModeEnum.AppendOverlay:
                    _overwriteMode = _APPEND_OVERLAY;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(overwriteMode), overwriteMode, null);
            }

            return this;
        }

        public IntegrationPointDto Build()
        {
            var integrationPointModel = new IntegrationPointDto
            {
                Type = _type,
                Name = _name,
                SourceProvider = _sourceProvider,
                SourceConfiguration = _sourceConfiguration,
                DestinationProvider = _destinationProvider,
                DestinationConfiguration = _destinationConfiguration,
                FieldMappings = _fieldMapping,
                SelectedOverwrite = _overwriteMode,
                Scheduler = new Scheduler
                {
                    EnableScheduler = false
                }
            };

            return integrationPointModel;
        }

        private IEnumerable<SourceProvider> GetSourceProviders()
        {
            var queryRequest = new QueryRequest();
            return _objectManager.Query<SourceProvider>(queryRequest);
        }

        private IEnumerable<DestinationProvider> GetDestinationProviders()
        {
            var queryRequest = new QueryRequest();
            return _objectManager.Query<DestinationProvider>(queryRequest);
        }
    }
}
