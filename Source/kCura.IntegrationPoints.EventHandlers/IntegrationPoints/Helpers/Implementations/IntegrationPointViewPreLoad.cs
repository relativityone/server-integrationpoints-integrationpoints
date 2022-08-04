using System;
using System.Collections.Generic;
using System.Dynamic;
using kCura.EventHandler;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using Newtonsoft.Json;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations
{
    public class IntegrationPointViewPreLoad : IIntegrationPointViewPreLoad
    {
        private readonly ICaseServiceContext _context;
        private readonly IIntegrationPointBaseFieldsConstants _fieldsConstants;
        private readonly IRelativityProviderConfiguration _relativityProviderSourceConfiguration;
        private readonly IRelativityProviderConfiguration _relativityProviderDestinationConfiguration;

        public IntegrationPointViewPreLoad(ICaseServiceContext context, IRelativityProviderConfiguration relativityProviderSourceConfiguration,
            IRelativityProviderConfiguration relativityProviderDestinationConfiguration, IIntegrationPointBaseFieldsConstants fieldsConstants)
        {
            _context = context;
            _relativityProviderSourceConfiguration = relativityProviderSourceConfiguration;
            _relativityProviderDestinationConfiguration = relativityProviderDestinationConfiguration;
            _fieldsConstants = fieldsConstants;
        }

        public void PreLoad(Artifact artifact)
        {
            if (IsRelativityProvider(artifact))
            {
                IDictionary<string,object> sourceConfiguration = GetSourceConfiguration(artifact);
                
                _relativityProviderSourceConfiguration.UpdateNames(sourceConfiguration, artifact);
                artifact.Fields[_fieldsConstants.SourceConfiguration].Value.Value = JsonConvert.SerializeObject(sourceConfiguration);
            }
            IDictionary<string, object> destinationConfiguration = GetDestinationConfiguration(artifact);

            _relativityProviderDestinationConfiguration.UpdateNames(destinationConfiguration, artifact);
            artifact.Fields[_fieldsConstants.DestinationConfiguration].Value.Value = JsonConvert.SerializeObject(destinationConfiguration);
        }

        private bool IsRelativityProvider(Artifact artifact)
        {
            int sourceProvider = (int) artifact.Fields[_fieldsConstants.SourceProvider].Value.Value;
            return _context.RelativityObjectManagerService.RelativityObjectManager.Read<SourceProvider>(int.Parse(sourceProvider.ToString())).Name == Constants.IntegrationPoints.RELATIVITY_PROVIDER_NAME;
        }

        private IDictionary<string, object> GetSourceConfiguration(Artifact artifact)
        {
            return GetConfiguration(artifact, _fieldsConstants.SourceConfiguration);
        }

        private IDictionary<string, object> GetDestinationConfiguration(Artifact artifact)
        {
            return GetConfiguration(artifact, _fieldsConstants.DestinationConfiguration);
        }

        private IDictionary<string, object> GetConfiguration(Artifact artifact, string configuration)
        {
            string sourceConfiguration = artifact.Fields[configuration].Value.Value.ToString();
            IDictionary<string, object> settings = new Dictionary<string, object>(JsonConvert.DeserializeObject<ExpandoObject>(sourceConfiguration), 
                StringComparer.CurrentCultureIgnoreCase);
            return settings;
        }
    }
}