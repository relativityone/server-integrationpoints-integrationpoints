﻿namespace kCura.IntegrationPoints.Common.Agent
{
	public interface IRemovableAgent
	{
		bool ToBeRemoved { get; set; }
	}
}