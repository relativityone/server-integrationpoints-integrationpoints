using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data;

namespace Relativity.IntegrationPoints.Tests.Integration
{
	public static class Const
	{
		public const string INTEGRATION_POINTS_APP_GUID = "DCF6E9D1-22B6-4DA3-98F6-41381E93C30C";
		
		public static class Agent
		{
			public static readonly Guid RELATIVITY_INTEGRATION_POINTS_AGENT_GUID =
				new Guid(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID);

			public static readonly int INTEGRATION_POINTS_AGENT_TYPE_ID = ArtifactProvider.NextId();

			public static readonly List<int> RESOURCE_GROUP_IDS = new List<int>
				{ArtifactProvider.NextId(), ArtifactProvider.NextId(), ArtifactProvider.NextId()};
		}

		public static class Provider
		{
			public const string _FAKE_PROVIDER = "9A33EBEA-B4F9-4427-8AD4-5D4F35F0405A";

			public static readonly Guid FakeProviderGuid = new Guid(_FAKE_PROVIDER);
		}

		public static class RdoGuids
		{
			public static class SyncConfiguration
			{
				public const string JobHistoryId = "Job History ID";
				public static readonly Guid JobHistoryIdGuid = new Guid("FF793525-29AB-40B2-A8AE-88E574EAD0DE");
				public const string Resuming = "Resuming";
				public static readonly Guid ResumingGuid = new Guid("A5B0959D-96CE-4CA0-9E4A-576132F29165");
			}
		}
	}
}
