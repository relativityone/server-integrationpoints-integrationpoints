using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using kCura.Agent;
using kCura.Agent.CustomAttributes;
using kCura.IntegrationPoints.Management;
using kCura.IntegrationPoints.Management.Installers;
using Relativity.API;

namespace kCura.IntegrationPoints.Manager
{
	[Name(_AGENT_NAME)]
	[Description("An agent for managing RIP jobs and overall RIP status.")]
	[Guid("F37752EB-D204-4F7D-9D6B-F55E64991846")]
	public class IntegrationPointsManagerAgent : AgentBase
	{
		private const int _LOGGING_MESSAGE_LEVEL = 10;
		private const string _AGENT_NAME = "Integration Points Manager";

		private readonly IContainerFactory _containerFactory;
		internal virtual IAPILog Logger => Helper.GetLoggerFactory().GetLogger();

		public override string Name => _AGENT_NAME;

		public IntegrationPointsManagerAgent() : this(new ContainerFactory())
		{
		}

		internal IntegrationPointsManagerAgent(IContainerFactory containerFactory)
		{
			_containerFactory = containerFactory;
		}

		public override void Execute()
		{
			RaiseMessage("Started", _LOGGING_MESSAGE_LEVEL);

			StartIntegrationPointsManager();

			RaiseMessage("Completed.", _LOGGING_MESSAGE_LEVEL);
		}

		private void StartIntegrationPointsManager()
		{
			try
			{
				using (var container = _containerFactory.Create(Helper))
				{
					var integrationPointsManager = container.Resolve<IIntegrationPointsManager>();
					integrationPointsManager.Start();
				}
			}
			catch (Exception ex)
			{
				LogError(ex);
			}
		}

		private void LogError(Exception exception)
		{
			Logger.LogError(exception, "Error occurred during agent execution.");
			RaiseError("Error occurred during agent execution.", exception.Message);
		}
	}
}