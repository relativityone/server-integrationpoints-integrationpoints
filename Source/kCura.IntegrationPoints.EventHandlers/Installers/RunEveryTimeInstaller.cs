using System;
using System.Runtime.InteropServices;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.Core.Telemetry;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Migrations;
using kCura.IntegrationPoints.SourceProviderInstaller;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
	[Description("Runs Post install Migration scripts for Integration Points.")]
	[RunOnce(false)]
	[Guid("fb52b882-22d3-481f-b000-b976c13baf49")]
	public class RunEveryTimeInstaller : PostInstallEventHandlerBase
	{
		protected override IAPILog CreateLogger()
		{
			return Helper.GetLoggerFactory().GetLogger().ForContext<RunEveryTimeInstaller>();
		}

		protected override string SuccessMessage => "Adding telemetry metrics completed successfully";

		protected override string GetFailureMessage(Exception ex)
		{
			return "Adding telemetry metrics failed";
		}

		protected override void Run()
		{
			new MigrationRunner(new EddsContext(Helper.GetDBContext(-1)), new WorkspaceContext(Helper.GetDBContext(Helper.GetActiveCaseID()))).Run();

			var telemetryManger = new TelemetryManager(Helper);
			telemetryManger.AddMetricProviders(new TelemetryMetricProvider(Helper));
			telemetryManger.AddMetricProviders(new ExportTelemetryMetricProvider(Helper));
			telemetryManger.InstallMetrics();
		}
	}
}