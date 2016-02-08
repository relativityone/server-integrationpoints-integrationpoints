using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using kCura.EventHandler;
using kCura.IntegrationPoints.SourceProviderInstaller;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
	[Guid("0846CFFC-757D-4F19-A439-24510012CCBE")]
	[EventHandler.CustomAttributes.Description("This is an event handler to register back provider after creating workspace using the template that has integration point installed.")]
	public class SourceProvidersImgrationEventHandler : IntegrationPointMigrationEventHandlerBase
	{
		public override Response Execute()
		{
			List<SourceProviderInstaller.SourceProvider> sourceProviders = GetSourceProvidersToInstall();
			SourceProvidersMigration migrationJob = new SourceProvidersMigration(sourceProviders, Helper);
			migrationJob.Execute();

			return new Response
			{
				Success = true,
				Message = "Successfully migrate source providers."
			};
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
				GUID = new Guid(provider.Identifier)
			}).ToList();

			return results;
		}

		/// Even private class needs a Guid :(. SAMO - 02/08/2016
		/// This private event handler will show on Relativity.
		[Guid("DDF4C569-AE1D-45F8-9E0F-740399BA059F")]
		private sealed class SourceProvidersMigration : IntegrationPointSourceProviderInstaller
		{
			private readonly List<SourceProviderInstaller.SourceProvider> _sourceProviders;

			public SourceProvidersMigration(List<SourceProviderInstaller.SourceProvider> sourceProvidersToMigrate, IEHHelper helper)
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