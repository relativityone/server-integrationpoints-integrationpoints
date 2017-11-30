using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using kCura.EventHandler;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.EventHandlers.Installers;
using kCura.IntegrationPoints.SourceProviderInstaller;
using kCura.IntegrationPoints.SourceProviderInstaller.Services;
using kCura.Relativity.Client.DTOs;
using kCura.Utility;
using Relativity.API;
using SourceProvider = kCura.IntegrationPoints.SourceProviderInstaller.SourceProvider;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
	[Guid("0846CFFC-757D-4F19-A439-24510012CCBE")]
	[EventHandler.CustomAttributes.Description("This is an event handler to register back provider after creating workspace using the template that has integration point installed.")]
	public class SourceProvidersMigrationEventHandler : IntegrationPointMigrationEventHandlerBase
	{
		internal IImportService Importer;
		private IAPILog _logger;

		public IAPILog Logger
		{
			get { return _logger ?? (_logger = Helper.GetLoggerFactory().GetLogger().ForContext<SourceProvidersMigrationEventHandler>()); }
			set { _logger = value; }
		}

		public override Response Execute()
		{
			try
			{
				List<SourceProviderInstaller.SourceProvider> sourceProviders = GetSourceProvidersToInstall();
				SourceProvidersMigration migrationJob = new SourceProvidersMigration(sourceProviders, Helper, Importer);
				return migrationJob.Execute();
			}
			catch (Exception e)
			{
				LogSourceProviderMigrationError(e);
				return new Response
				{
					Message = e.Message,
					Exception = e,
					Success = false
				};
			}
		}

		private List<SourceProviderInstaller.SourceProvider> GetSourceProvidersToInstall()
		{
			List<Data.SourceProvider> sourceProviders = GetSourceProvidersFromPreviousWorkspace();

			List<SourceProviderInstaller.SourceProvider> results = sourceProviders.Select(provider => new SourceProviderInstaller.SourceProvider()
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
			Query<RDO> query = new Query<RDO>();
			query.Fields = GetAllSourceProviderFields();

			List<Data.SourceProvider> sourceProviderRdos = WorkspaceTemplateServiceContext.RsapiService.SourceProviderLibrary.Query(query);

			if (sourceProviderRdos == null || sourceProviderRdos.Count == 0)
			{
				Logger.LogError("Could not retrieve source providers from previous workspace {PreviousWorkspaceArtifactID}", WorkspaceTemplateServiceContext.WorkspaceID);
				return new List<Data.SourceProvider>();
			}
			return sourceProviderRdos;
		}

		private List<Relativity.Client.DTOs.FieldValue> GetAllSourceProviderFields()
		{
			List<Relativity.Client.DTOs.FieldValue> fields = BaseRdo.GetFieldMetadata(typeof(Data.SourceProvider)).Select(pair => new Relativity.Client.DTOs.FieldValue(pair.Value.FieldGuid)).ToList();
			return fields;
		}

		/// Even private class needs a Guid :(. SAMO - 02/08/2016
		/// This private event handler will show on Relativity.
		[Guid("DDF4C569-AE1D-45F8-9E0F-740399BA059F")]
		private sealed class SourceProvidersMigration : IntegrationPointSourceProviderInstaller
		{
			private readonly List<SourceProvider> _sourceProviders;

			public SourceProvidersMigration(List<SourceProvider> sourceProvidersToMigrate, IEHHelper helper, IImportService importService)
			{
				_sourceProviders = sourceProvidersToMigrate;
				Helper = helper;
				ImportService = importService;
			}

			public override IDictionary<Guid, SourceProvider> GetSourceProviders()
			{
				return _sourceProviders.ToDictionary(provider => provider.GUID);
			}
		}

		#region Logging

		private void LogSourceProviderMigrationError(Exception e)
		{
			Logger.LogError(e, "Failed to migrate Source Provider.");
		}

		#endregion
	}
}