using kCura.EventHandler;
using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Services;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
		private const int _SEND_INSTALL_REQUEST_MAX_RETRIES_NUMBER = 3;

		private readonly Lazy<IAPILog> _logggerLazy;

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
		public sealed override Response Execute()
		{
			IDictionary<Guid, SourceProvider> sourceProviders = GetSourceProviders();
			Logger.LogDebug("Starting Post-installation process for {sourceProviders} provider", sourceProviders.Values.Select(item => item.Name));
			if (sourceProviders.Count == 0)
			{
				// TODO
				//throw new IntegrationPointsException($"Provider does not implement the contract (Empty source provider list retrieved from {GetType().Name} class)");
			}

			Exception thrownException = null;
			try
			{
				OnRaisePostInstallPreExecuteEvent();
				InstallSourceProvider(sourceProviders);
			}
			catch (Exception e)
			{
				thrownException = e;
				throw;
			}
			finally
			{
				bool isSuccess = thrownException == null;
				OnRaisePostInstallPostExecuteEvent(isSuccess, thrownException);
			}

			return new Response
			{
				Success = true
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

			InstallSourceProvider(sourceProviders);
		}

		internal virtual void InstallSourceProvider(IEnumerable<SourceProvider> sourceProviders) // TODO private protected
		{
			var request = new InstallProviderRequest
			{
				WorkspaceID = Helper.GetActiveCaseID(),
				ProvidersToInstall = sourceProviders.Select(ConvertSourceProviderToDto).ToList()
			};

			bool isProviderInstalled = SendInstallProviderRequest(request);
			if (!isProviderInstalled)
			{
				// TODO throw proper exception
			}
		}

		private bool SendInstallProviderRequest(InstallProviderRequest request)
		{
			try
			{
				return SendInstallProviderRequestWithRetriesAsync(request).GetAwaiter().GetResult();
			}
			catch (Exception ex)
			{
				Logger.LogError(ex, "Installing source provider failed");
				throw; // TODO throw proper exception
			}
		}

		/// <summary>
		/// We cannot use Polly, because it would require adding external dependency to our SDK
		/// </summary>
		private async Task<bool> SendInstallProviderRequestWithRetriesAsync(InstallProviderRequest request, int attemptNumber = 0)
		{
			try
			{
				using (var providerManager = Helper.GetServicesManager().CreateProxy<IProviderManager>(ExecutionIdentity.CurrentUser))
				{
					return await providerManager.InstallProviderAsync(request).ConfigureAwait(false);
				}
			}
			catch (Exception ex)
			{
				Logger.LogWarning(ex, "Installing provider failed, attempt {attemptNumber} out of {numberOfRetries}.",
					attemptNumber,
					_SEND_INSTALL_REQUEST_MAX_RETRIES_NUMBER
				);
				if (attemptNumber == _SEND_INSTALL_REQUEST_MAX_RETRIES_NUMBER)
				{
					throw;
				}
			}

			return await SendInstallProviderRequestWithRetriesAsync(request, attemptNumber + 1);
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
			// TODO log error
			RaisePostInstallPreExecuteEvent?.Invoke();
		}

		/// <summary>
		/// Occurs after the registration process completes.
		/// </summary>
		/// <param name="isInstalled">Indicates whether the data source providers were installed.</param>
		/// <param name="ex">An exception thrown when errors occur during the installation of the data source provider.</param>
		protected void OnRaisePostInstallPostExecuteEvent(bool isInstalled, Exception ex)
		{
			// TODO log error
			RaisePostInstallPostExecuteEvent?.Invoke(isInstalled, ex);
		}
	}
}