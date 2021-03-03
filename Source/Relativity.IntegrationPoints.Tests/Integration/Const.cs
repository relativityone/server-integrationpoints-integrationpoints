using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data;

namespace Relativity.IntegrationPoints.Tests.Integration
{
	public static class Const
	{
		public static class Agent
		{
			public static readonly Guid _RELATIVITY_INTEGRATION_POINTS_AGENT_GUID =
				new Guid(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID);

			public static readonly int _INTEGRATION_POINTS_AGENT_TYPE_ID = Artifact.NextId();

			public static readonly List<int> _RESOURCE_GROUP_IDS = new List<int>
				{Artifact.NextId(), Artifact.NextId(), Artifact.NextId()};
		}
	}
}
