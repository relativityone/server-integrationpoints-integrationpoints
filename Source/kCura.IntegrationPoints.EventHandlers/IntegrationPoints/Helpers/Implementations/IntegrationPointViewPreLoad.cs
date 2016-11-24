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
		private readonly IRelativityProviderSourceConfiguration _relativityProviderSourceConfiguration;

		public IntegrationPointViewPreLoad(ICaseServiceContext context, IRelativityProviderSourceConfiguration relativityProviderSourceConfiguration,
			IIntegrationPointBaseFieldsConstants fieldsConstants)
		{
			_context = context;
			_relativityProviderSourceConfiguration = relativityProviderSourceConfiguration;
			_fieldsConstants = fieldsConstants;
		}

		public void PreLoad(Artifact artifact)
		{
			if (IsRelativityProvider(artifact))
			{
				var settings = GetSourceConfiguration(artifact);

				_relativityProviderSourceConfiguration.UpdateNames(settings);

				artifact.Fields[_fieldsConstants.SourceConfiguration].Value.Value = JsonConvert.SerializeObject(settings);
			}
		}

		private bool IsRelativityProvider(Artifact artifact)
		{
			int sourceProvider = (int) artifact.Fields[_fieldsConstants.SourceProvider].Value.Value;
			return _context.RsapiService.SourceProviderLibrary.Read(int.Parse(sourceProvider.ToString())).Name == Constants.IntegrationPoints.RELATIVITY_PROVIDER_NAME;
		}

		private IDictionary<string, object> GetSourceConfiguration(Artifact artifact)
		{
			string sourceConfiguration = artifact.Fields[_fieldsConstants.SourceConfiguration].Value.Value.ToString();
			IDictionary<string, object> settings = JsonConvert.DeserializeObject<ExpandoObject>(sourceConfiguration);
			return settings;
		}
	}
}