﻿using System.Collections.Generic;

namespace kCura.IntegrationPoints.Domain.Logging
{
	public class AgentCorrelationContext : BaseCorrelationContext
	{
		public long JobId { get; set; }
		public long? RootJobId { get; set; }

		public override Dictionary<string, object> ToDictionary()
		{
			Dictionary<string, object> baseProperties = base.ToDictionary();
			baseProperties.Add(nameof(JobId), JobId);
			baseProperties.Add(nameof(RootJobId), RootJobId);
			return baseProperties;
		}

		public override void SetValuesFromDictionary(Dictionary<string, object> dictionary)
		{
			if (dictionary == null)
			{
				return;
			}
			base.SetValuesFromDictionary(dictionary);
			JobId = GetValueOrDefault<long>(dictionary, nameof(JobId));
			RootJobId = GetValueOrDefault<long?>(dictionary, nameof(RootJobId));
		}
	}
}