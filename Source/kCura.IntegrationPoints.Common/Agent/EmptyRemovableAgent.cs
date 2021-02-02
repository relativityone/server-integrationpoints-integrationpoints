﻿namespace kCura.IntegrationPoints.Common.Agent
{
	/// <summary>
	/// This class is intended to use by components that do not require Agent instance running, such as Event Handlers or Web.
	/// </summary>
	public class EmptyRemovableAgent : IRemovableAgent
	{
		public bool ToBeRemoved { get; set; } = false;
	}
}