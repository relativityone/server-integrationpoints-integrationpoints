using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.EventHandlers.Commands.Metrics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.EventHandlers.Installers.Metrics
{
	[Description("Register Schedule Sync metrics")]
	[RunOnce(true)]
	[Guid("87E4A98B-1EA8-4776-A12C-9AA7F1E29589")]
	public class RegisterScheduleSyncSUMMetricsEventHandler : PostInstallEventHandlerBase
	{
		private readonly IRegisterSumMetricsCommandFactory _commandFactory;

		public RegisterScheduleSyncSUMMetricsEventHandler(IRegisterSumMetricsCommandFactory commandFactory)
		{
			_commandFactory = commandFactory;
		}

		protected override string SuccessMessage => "Schedule Sync Metrics has been successfully installed";

		protected override string GetFailureMessage(Exception ex)
		{
			const string errorMsg = "Schedule Sync Metrics installation failed";

			Logger.LogError(ex, errorMsg);

			return errorMsg;
		}

		protected override void Run()
		{
			_commandFactory
				.CreateCommand<RegisterScheduleJobSumMetricsCommand>()
				.Execute();
		}
	}
}
