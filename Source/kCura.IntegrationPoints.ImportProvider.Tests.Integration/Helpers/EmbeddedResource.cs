using System.Collections.Generic;
using System.IO;
using System.Reflection;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.ImportProvider.Tests.Integration.Helpers
{
    public static class EmbeddedResource
    {
        private const string _RESOURCE_STREAM_ROOT = "kCura.IntegrationPoints.ImportProvider.Tests.Integration.SettingsResources.";
        private const string _FIELDMAP_RESOURCE = "FieldMaps";
        private const string _IMPORT_SETTINGS_RESOURCE = "ImportSettings";
        private const string _IMPORTPROVIDER_SETTINGS_RESOURCE = "ImportProviderSettings";
        private static ISerializer _serializer = null;
        private static ISerializer Serializer
        {
            get
            {
                if (_serializer == null)
                {
                    _serializer = RipJsonSerializer.CreateWithoutLogger();
                }
                return _serializer;
            }
        }

        public static List<FieldMap> FieldMaps(string resourceName)
        {
            return Serializer.Deserialize<List<FieldMap>>(EmbeddedResourceStreamReader(_FIELDMAP_RESOURCE, resourceName).ReadToEnd());
        }

        public static ImportProviderSettings ImportProviderSettings(string resourceName)
        {
            return Serializer.Deserialize<ImportProviderSettings>(EmbeddedResourceStreamReader(_IMPORTPROVIDER_SETTINGS_RESOURCE, resourceName).ReadToEnd());
        }

        public static DestinationConfiguration DestinationConfiguration(string resourceName)
        {
            return Serializer.Deserialize<DestinationConfiguration>(EmbeddedResourceStreamReader(_IMPORT_SETTINGS_RESOURCE, resourceName).ReadToEnd());
        }

        private static StreamReader EmbeddedResourceStreamReader(string category, string resourceName)
        {
            return new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(_RESOURCE_STREAM_ROOT + category  + "." + category + resourceName + ".json"));
        }
    }
}
