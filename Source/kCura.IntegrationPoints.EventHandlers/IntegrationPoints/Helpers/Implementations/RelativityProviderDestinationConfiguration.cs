using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Contracts;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;
using Artifact = kCura.EventHandler.Artifact;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations
{
	public class RelativityProviderDestinationConfiguration : RelativityProviderConfiguration
	{
		private const string _ARTIFACT_TYPE_NAME = "ArtifactTypeName";

		public RelativityProviderDestinationConfiguration(IEHHelper helper) : base(helper)
		{
		}

		public override void UpdateNames(IDictionary<string, object> settings, Artifact artifact)
		{
			try
			{
				int transferredObjArtifactTypeId = ParseValue<int>(settings,
					nameof(DestinationConfiguration.ArtifactTypeId));

				using (IRSAPIClient client = GetRsapiClient(Helper.GetActiveCaseID()))
				{
					ObjectType objectType = new RSAPIRdoQuery(client).GetType(transferredObjArtifactTypeId);
					settings[_ARTIFACT_TYPE_NAME] = objectType.Name;
				}
			}
			catch (Exception ex)
			{
				Helper.GetLoggerFactory().GetLogger().LogError(ex, "Cannot retrieve object type name for artifact type id");
				settings[_ARTIFACT_TYPE_NAME] = "RDO";
			}
		}
	}
}