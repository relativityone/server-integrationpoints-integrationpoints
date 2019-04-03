using kCura.IntegrationPoints.Core.Services;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using SourceProvider = kCura.IntegrationPoints.Contracts.SourceProvider;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
	[Guid("0846CFFC-757D-4F19-A439-24510012CCBE")]
	[EventHandler.CustomAttributes.Description("This is an event handler to register back provider after creating workspace using the template that has integration point installed.")]
	public class SourceProvidersMigrationEventHandler : IntegrationPointMigrationEventHandlerBase
	{
		private IAPILog _logger;

		private IAPILog Logger =>
			_logger
			?? (_logger = Helper.GetLoggerFactory().GetLogger().ForContext<SourceProvidersMigrationEventHandler>());

		public SourceProvidersMigrationEventHandler()
		{ }

		public SourceProvidersMigrationEventHandler(IErrorService errorService) : base(errorService)
		{ }

		protected override void Run()
		{
			List<SourceProvider> sourceProviders = GetSourceProvidersToInstall();
			var migrationJob = new SourceProvidersMigration(sourceProviders, Helper);
			migrationJob.Execute();
		}

		protected override string SuccessMessage => "Source Provider migrated successfully.";

		protected override string GetFailureMessage(Exception ex) => "Failed to migrate Source Provider.";

		private List<SourceProvider> GetSourceProvidersToInstall()
		{
			List<Data.SourceProvider> sourceProviders = GetSourceProvidersFromPreviousWorkspace();

			List<SourceProvider> results = sourceProviders.Select(provider => new SourceProvider
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
			List<Data.SourceProvider> sourceProviderRdos = WorkspaceTemplateServiceContext.RsapiService.RelativityObjectManager.Query<Data.SourceProvider>(new QueryRequest());

			if (sourceProviderRdos == null || sourceProviderRdos.Count == 0)
			{
				Logger.LogError("Could not retrieve source providers from previous workspace {PreviousWorkspaceArtifactID}", WorkspaceTemplateServiceContext.WorkspaceID);
				return new List<Data.SourceProvider>();
			}
			return sourceProviderRdos;
		}

		/// Even private class needs a Guid :(. SAMO - 02/08/2016
		/// This private event handler will show on Relativity.
		[Guid("DDF4C569-AE1D-45F8-9E0F-740399BA059F")]
		private sealed class SourceProvidersMigration : InternalSourceProviderInstaller
		{
			private readonly List<SourceProvider> _sourceProviders;

			public SourceProvidersMigration(List<SourceProvider> sourceProvidersToMigrate, IEHHelper helper)
			{
				_sourceProviders = sourceProvidersToMigrate;
				Helper = helper;
			}

			public override IDictionary<Guid, SourceProvider> GetSourceProviders()
			{
				return _sourceProviders.ToDictionary(provider => provider.GUID);
			}
		}
	}
}