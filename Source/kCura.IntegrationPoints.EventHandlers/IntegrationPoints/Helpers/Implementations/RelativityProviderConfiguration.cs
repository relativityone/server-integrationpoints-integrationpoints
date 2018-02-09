using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data.RSAPIClient;
using kCura.Relativity.Client;
using Relativity.API;
using Artifact = kCura.EventHandler.Artifact;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations
{
	public abstract class RelativityProviderConfiguration : IRelativityProviderConfiguration
	{
		protected RelativityProviderConfiguration(IEHHelper helper)
		{
			Helper = helper;
		}

		protected IEHHelper Helper { get; }

		public abstract void UpdateNames(IDictionary<string, object> settings, Artifact artifact);

		protected static T ParseValue<T>(IDictionary<string, object> settings, string parameterName)
		{
			if (!settings.ContainsKey(parameterName) || settings[parameterName] == null)
			{
				return default(T);
			}
			return (T) Convert.ChangeType(settings[parameterName], typeof(T));
		}

		protected virtual IRSAPIClient GetRsapiClient(int workspaceArtifactId)
		{
			var rsapiClientFactory = new RsapiClientFactory();
			IRSAPIClient rsapiClient = rsapiClientFactory.CreateUserClient(Helper);
			rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;
			return rsapiClient;
		}
	}
}