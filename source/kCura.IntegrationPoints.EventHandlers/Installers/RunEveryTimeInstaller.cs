using kCura.EventHandler;
using System.Runtime.InteropServices;
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
			
			new MigrationRunner(new EddsContext(Helper.GetDBContext(-1)), new WorkspaceContext(base.Helper.GetDBContext(base.Helper.GetActiveCaseID()))).Run();
			return new Response { Success = true };
		}
	}
}
