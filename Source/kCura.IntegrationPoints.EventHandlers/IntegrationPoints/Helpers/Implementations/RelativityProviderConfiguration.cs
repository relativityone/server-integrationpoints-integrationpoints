using System;
using System.Collections.Generic;
using kCura.Relativity.Client;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations
{
	public abstract class RelativityProviderConfiguration  : IRelativityProviderConfiguration
	{
		protected IEHHelper Helper { get; }
	
		protected RelativityProviderConfiguration(IEHHelper helper)
		{
			Helper = helper;
		}

		public abstract void UpdateNames(IDictionary<string, object> settings);

		protected static T ParseValue<T>(IDictionary<string, object> settings, string parameterName)
		{
			if (!settings.ContainsKey(parameterName))
			{
				return default(T);
			}
			return (T)Convert.ChangeType(settings[parameterName], typeof(T));
		}

		protected virtual IRSAPIClient GetRsapiClient(int workspaceArtifactId)
		{
			IRSAPIClient rsapiClient = Helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser);
			rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;
			return rsapiClient;
		}
	}
}
