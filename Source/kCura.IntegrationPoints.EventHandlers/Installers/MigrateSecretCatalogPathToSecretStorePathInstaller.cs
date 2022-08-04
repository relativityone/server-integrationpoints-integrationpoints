using System;
using System.Runtime.InteropServices;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.EventHandlers.Commands;
using kCura.IntegrationPoints.EventHandlers.Commands.Context;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
    [Description("Migrate secrets from Secret Catalog to Secret Store")]
    [RunOnce(true)]
    [Guid("5FC77ECF-F263-4A0A-B3B8-82E0EB18BE57")]
    public class MigrateSecretCatalogPathToSecretStorePathInstaller : PostInstallEventHandlerBase, IEventHandler
    {
        protected override string SuccessMessage => "Successfully migrated secrets from Secret Catalog to Secret Store.";
        protected override string GetFailureMessage(Exception ex)
        {
            return "Unable to migrate secrets from Secret Catalog to Secret Store.";
        }

        protected override void Run()
        {
            var executor = new EventHandlerExecutor();
            executor.Execute(this);
        }
        protected override IAPILog CreateLogger()
        {
            return Helper.GetLoggerFactory().GetLogger().ForContext<MigrateSecretCatalogPathToSecretStorePathInstaller>();
        }

        public IEHContext Context => new EHContext()
        {
            Helper = Helper
        };

        public Type CommandType => typeof(MigrateSecretCatalogPathToSecretStorePathCommand);
    }
}
