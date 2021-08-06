using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using kCura.EventHandler;
using kCura.IntegrationPoints.Core.Provider;
using kCura.IntegrationPoints.Core.Services;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts;
using Relativity.IntegrationPoints.SourceProviderInstaller;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
    [Guid("0846CFFC-757D-4F19-A439-24510012CCBE")]
    [EventHandler.CustomAttributes.Description("This is an event handler to register back provider after creating workspace using the template that has integration point installed.")]
    public class SourceProvidersMigrationEventHandler : IntegrationPointMigrationEventHandlerBase
    {
        private readonly IRipProviderInstaller _ripProviderInstaller;

        public SourceProvidersMigrationEventHandler()
        { }

        public SourceProvidersMigrationEventHandler(IErrorService errorService,
            IRipProviderInstaller ripProviderInstaller)
            : base(errorService)
        {
            _ripProviderInstaller = ripProviderInstaller;
        }

        protected override void Run()
        {
            Logger.LogInformation($"Running {nameof(SourceProvidersMigrationEventHandler)}");
            List<SourceProvider> sourceProviders = GetSourceProvidersToInstall();
            var migrationJob = new SourceProvidersMigration(sourceProviders, Helper, _ripProviderInstaller, TemplateWorkspaceID, Logger);
            Logger.LogInformation("Executing Source Providers migration job");
            Response migrationJobResult = migrationJob.Execute();
            Logger.LogInformation("Source Providers migration job execution result success: {result}", migrationJobResult.Success);
            if (!migrationJobResult.Success)
            {
                throw new InvalidSourceProviderException(migrationJobResult.Message, migrationJobResult.Exception);
            }
        }

        protected override string SuccessMessage => "Source Provider migrated successfully.";

        protected override string GetFailureMessage(Exception ex) => "Failed to migrate Source Provider.";

        private List<SourceProvider> GetSourceProvidersToInstall()
        {
            List<Data.SourceProvider> sourceProviders = GetSourceProvidersFromPreviousWorkspace();

	        List<SourceProvider> results = sourceProviders.Select(
		        provider => new SourceProvider
		        {
			        Name = provider.Name,
			        Url = provider.SourceConfigurationUrl,
			        ViewDataUrl = provider.ViewConfigurationUrl,
			        ApplicationGUID = new Guid(provider.ApplicationIdentifier),
			        GUID = new Guid(provider.Identifier),
			        Configuration = provider.Config
		        }).ToList();

            return results;
        }

        protected virtual List<Data.SourceProvider> GetSourceProvidersFromPreviousWorkspace()
        {
            Logger.LogInformation("Retrieving Source Providers from Template Workspace Artifact ID: {templateWorkspaceArtifactId}", WorkspaceTemplateServiceContext.WorkspaceID);
            List<Data.SourceProvider> sourceProviderRdos = WorkspaceTemplateServiceContext.RelativityObjectManagerService.RelativityObjectManager.Query<Data.SourceProvider>(new QueryRequest());

            if (sourceProviderRdos == null || sourceProviderRdos.Count == 0)
            {
                Logger.LogError("Could not retrieve source providers from previous workspace {PreviousWorkspaceArtifactID}", WorkspaceTemplateServiceContext.WorkspaceID);
                return new List<Data.SourceProvider>();
            }

            Logger.LogInformation("Found {count} Source Providers in Template Workspace Artifact ID: {templateWorkspaceArtifactId}", sourceProviderRdos.Count, WorkspaceTemplateServiceContext.WorkspaceID);

            return sourceProviderRdos;
        }

        /// Even private class needs a Guid :(. SAMO - 02/08/2016
        /// This private event handler will show on Relativity.
        [Guid("DDF4C569-AE1D-45F8-9E0F-740399BA059F")]
        private sealed class SourceProvidersMigration : InternalSourceProviderInstaller
        {
            private readonly List<SourceProvider> _sourceProviders;
            private readonly int _templateWorkspaceId;
            private readonly IAPILog _logger;

	        public SourceProvidersMigration(
		        List<SourceProvider> sourceProvidersToMigrate,
		        IEHHelper helper, 
		        IRipProviderInstaller providerInstaller,
		        int templateWorkspaceId,
		        IAPILog logger)
		        : base(providerInstaller)
	        {
		        _sourceProviders = sourceProvidersToMigrate;
		        _templateWorkspaceId = templateWorkspaceId;
		        _logger = logger;
                Helper = helper;
            }

	        public override IDictionary<Guid, SourceProvider> GetSourceProviders()
	        {
		        List<SourceProvider> deduplicatedProviders = new List<SourceProvider>();
		        foreach (SourceProvider sourceProvider in _sourceProviders)
		        {
			        if (deduplicatedProviders.All(x => x.GUID != sourceProvider.GUID))
			        {
				        deduplicatedProviders.Add(sourceProvider);
			        }
                }

		        if (_sourceProviders.Count > deduplicatedProviders.Count)
		        {
			        // REL-539111
                    _logger.LogWarning("There are duplicated entries in SourceProvider database table in Template Workspace Artifact ID: {templateWorkspaceArtifactId}", _templateWorkspaceId);
		        }

                return deduplicatedProviders.ToDictionary(provider => provider.GUID);
            }
        }
    }
}