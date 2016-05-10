using System;

namespace kCura.IntegrationPoints.Agent.Exceptions
{

	[Serializable]
	public class AgentDropJobException : Exception
	{
		public AgentDropJobException()
		{
		}

		public AgentDropJobException(string message) : base(message)
		{
		}

	}
}
