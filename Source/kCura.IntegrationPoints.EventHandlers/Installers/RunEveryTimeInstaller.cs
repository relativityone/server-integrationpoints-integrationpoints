using System;
using System.Runtime.InteropServices;
using kCura.EventHandler;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.Core.Telemetry;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Migrations;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
	[Description("Runs Post install Migration scripts for Integration Points.")]
	[RunOnce(false)]
	[Guid("fb52b882-22d3-481f-b000-b976c13baf49")]
	public class RunEveryTimeInstaller : PostInstallEventHandler
	{
		public override Response Execute()
		{
			var response = new Response {Success = true};
			try
			{
				new MigrationRunner(new EddsContext(Helper.GetDBContext(-1)), new WorkspaceContext(Helper.GetDBContext(Helper.GetActiveCaseID()))).Run();

				var telemetryManger = new TelemetryManager(Helper);
				telemetryManger.AddMetricProviders(new TelemetryMetricProvider());
				telemetryManger.AddMetricProviders(new ExportTelemetryMetricProvider());
				telemetryManger.InstallMetrics();
			}
			catch (Exception ex)
			{
				LogExecutingError(ex);
				response.Success = false;
				response.Message = ex.Message;
				response.Exception = ex;
			}
			return response;
		}

		#region Logging

		private void LogExecutingError(Exception ex)
		{
			var logger = Helper.GetLoggerFactory().GetLogger().ForContext<RunEveryTimeInstaller>();
			logger.LogError(ex, "Failed to execute RunEveryTimeInstaller.");
		}

		#endregion
	}
}