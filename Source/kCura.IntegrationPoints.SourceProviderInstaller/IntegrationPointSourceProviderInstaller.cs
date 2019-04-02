using kCura.EventHandler;
using kCura.IntegrationPoints.Services;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts;

namespace kCura.IntegrationPoints.SourceProviderInstaller
{
	/// <summary>
	/// Occurs immediately before the execution of a Post Install event handler.
	/// </summary>
	public delegate void PostInstallPreExecuteEvent();

	/// <summary>
	/// Occurs after all source providers are registered.
	/// </summary>
	/// <param name="isInstalled">Indicates whether the source providers were installed.</param>
	/// <param name="ex">An exception thrown when errors occur during the installation of a data source provider.</param>
	public delegate void PostInstallPostExecuteEvent(bool isInstalled, Exception ex);

	/// <summary>
	/// Registers the new data source providers with Relativity Integration Points.
	/// </summary>
	public abstract class IntegrationPointSourceProviderInstaller : PostInstallEventHandler
	{
		private readonly Lazy<IAPILog> _logggerLazy;

		// TODO handle success errro message: PostInstallEventHandlerBase

		/// <summary>
		/// Raised immediately before the execution of a Post Install event handler.
		/// </summary>
		public event PostInstallPreExecuteEvent RaisePostInstallPreExecuteEvent;
		/// <summary>
		/// Raised after all source providers are registered.
		/// </summary>
		public event PostInstallPostExecuteEvent RaisePostInstallPostExecuteEvent;

		private IAPILog Logger => _logggerLazy.Value;

		/// <summary>
		/// Initializes <see cref="IntegrationPointSourceProviderInstaller"/>
		/// </summary>
		protected IntegrationPointSourceProviderInstaller()
		{
			_logggerLazy = new Lazy<IAPILog>(
				() => Helper.GetLoggerFactory().GetLogger().ForContext<IntegrationPointSourceProviderInstaller>()
			);
		}

		/// <summary>
		/// Retrieves the data source providers for registration with the application.
		/// </summary>
		/// <returns>The data source providers for registration.</returns>
		public abstract IDictionary<Guid, SourceProvider> GetSourceProviders();

		/// <inheritdoc cref="PostInstallEventHandler"/>
		public sealed override Response Execute() // TODO improve error handling
		{
			IDictionary<Guid, SourceProvider> sourceProviders = GetSourceProviders();
			Logger.LogDebug("Starting Post-installation process for {sourceProviders} provider", sourceProviders.Values.Select(item => item.Name));
			if (sourceProviders.Count == 0)
			{
				//throw new IntegrationPointsException($"Provider does not implement the contract (Empty source provider list retrieved from {GetType().Name} class)"); // tODO
			}


			bool isSuccess = false;
			Exception ex = null;
			try
			{
				OnRaisePostInstallPreExecuteEvent();
				InstallSourceProvider(sourceProviders);

				isSuccess = true;
			}
			catch (Exception e)
			{
				ex = e;
				isSuccess = false;
				throw;
			}
			finally
			{
				OnRaisePostInstallPostExecuteEvent(isSuccess, ex);
			}
			return new Response
			{
				Success = isSuccess
			};
		}

		private void InstallSourceProvider(IDictionary<Guid, SourceProvider> providers)
		{
			if (!providers.Any())
			{
				throw new InvalidSourceProviderException("No Source Providers passed.");
			}

			IEnumerable<SourceProvider> sourceProviders = providers.Select(x => new SourceProvider
			{
				GUID = x.Key,
				ApplicationID = base.ApplicationArtifactId,
				ApplicationGUID = x.Value.ApplicationGUID,
				Name = x.Value.Name,
				Url = x.Value.Url,
				ViewDataUrl = x.Value.ViewDataUrl,
				Configuration = x.Value.Configuration
			});

			var request = new InstallProviderRequest
			{
				WorkspaceID = Helper.GetActiveCaseID(),
				ProvidersToInstall = sourceProviders.Select(ConvertSourceProviderToDto).ToList()
			};

			// TODO add retries here, without Polly
			using (var providerManager =
				Helper.GetServicesManager().CreateProxy<IProviderManager>(ExecutionIdentity.CurrentUser))
			{
				try
				{
					bool isSuccess = providerManager.InstallProviderAsync(request).GetAwaiter().GetResult();
				}
				catch (Exception ex)
				{
					throw ex;
				}
			}
		}

		private ProviderToInstallDto ConvertSourceProviderToDto(SourceProvider provider)
		{
			if (provider == null)
			{
				return null;
			}

			return new ProviderToInstallDto
			{
				Name = provider.Name,
				ApplicationGUID = provider.ApplicationGUID,
				ApplicationID = provider.ApplicationID,
				GUID = provider.GUID,
				Url = provider.Url,
				ViewDataUrl = provider.ViewDataUrl,
				Configuration = ConvertConfiguration(provider.Configuration)
			};
		}

		private ProviderToInstallConfigurationDto ConvertConfiguration(SourceProviderConfiguration configuration)
		{
			if (configuration == null)
			{
				return null;
			}

			return new ProviderToInstallConfigurationDto
			{
				CompatibleRdoTypes = configuration.CompatibleRdoTypes,
				AlwaysImportNativeFileNames = configuration.AlwaysImportNativeFileNames,
				AlwaysImportNativeFiles = configuration.AlwaysImportNativeFiles,
				OnlyMapIdentifierToIdentifier = configuration.OnlyMapIdentifierToIdentifier,
				AllowUserToMapNativeFileField = configuration.AvailableImportSettings?.AllowUserToMapNativeFileField ?? false
			};
		}

		/// <summary>
		/// Raises an event prior to the execution of a Post Install event handler.
		/// </summary>
		protected void OnRaisePostInstallPreExecuteEvent()
		{
			RaisePostInstallPreExecuteEvent?.Invoke();
		}

		/// <summary>
		/// Occurs after the registration process completes.
		/// </summary>
		/// <param name="isInstalled">Indicates whether the data source providers were installed.</param>
		/// <param name="ex">An exception thrown when errors occur during the installation of the data source provider.</param>
		protected void OnRaisePostInstallPostExecuteEvent(bool isInstalled, Exception ex)
		{
			RaisePostInstallPostExecuteEvent?.Invoke(isInstalled, ex);
		}
	}
}