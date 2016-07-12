﻿using System;
using kCura.EventHandler;
using System.Runtime.InteropServices;
using kCura.IntegrationPoints.Core.Telemetry;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Migrations;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
	[kCura.EventHandler.CustomAttributes.Description("Runs Post install Migration scripts for Integration Points.")]
	[kCura.EventHandler.CustomAttributes.RunOnce(false)]
	[Guid("fb52b882-22d3-481f-b000-b976c13baf49")]
	public class RunEveryTimeInstaller : kCura.EventHandler.PostInstallEventHandler
	{
		public override Response Execute()
		{
			var response = new Response { Success = true };
			try
			{
				new MigrationRunner(new EddsContext(Helper.GetDBContext(-1)), new WorkspaceContext(base.Helper.GetDBContext(base.Helper.GetActiveCaseID()))).Run();

				var telemetryManger = new TelemetryManager(base.Helper);
				telemetryManger.AddMetricProviders(new TelemetryMetricProvider());
				telemetryManger.AddMetricProviders(new ExportTelemetryMetricProvider());
				telemetryManger.InstallMetrics();
			}
			catch (Exception ex)
			{
				response.Success = false;
				response.Message = ex.Message;
				response.Exception = ex;
			}
			return response;
		}
	}
}
