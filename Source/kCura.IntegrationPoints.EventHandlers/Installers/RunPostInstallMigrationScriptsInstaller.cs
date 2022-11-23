using System;
using System.Runtime.InteropServices;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.Data.DbContext;
using kCura.IntegrationPoints.Data.Migrations;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
    [Description("Runs Post install Migration scripts for Integration Points.")]
    [RunOnce(false)]
    [Guid("F8FCD986-D363-4E31-9016-0FD103230625")]
    public class RunPostInstallMigrationScriptsInstaller : PostInstallEventHandlerBase
    {
        protected override IAPILog CreateLogger()
        {
            return Helper.GetLoggerFactory().GetLogger().ForContext<RunPostInstallMigrationScriptsInstaller>();
        }

        protected override string SuccessMessage => "Post install migration scripts for Integration Points completed successfully";

        protected override string GetFailureMessage(Exception ex)
        {
            return "Post install migration scripts for Integration Points failed";
        }

        protected override void Run()
        {
            IDbContextFactory dbContextFactory = new DbContextFactory(Helper);

            IEddsDBContext eddsDbContext = dbContextFactory.CreatedEDDSDbContext();
            IWorkspaceDBContext workspaceDbContext = dbContextFactory.CreateWorkspaceDbContext(Helper.GetActiveCaseID());

            new MigrationRunner(eddsDbContext, workspaceDbContext).Run();
        }
    }
}
