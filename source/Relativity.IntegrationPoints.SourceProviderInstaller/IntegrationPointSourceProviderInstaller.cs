using System;
using System.Collections.Generic;
using kCura.EventHandler;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts;
using Relativity.IntegrationPoints.SourceProviderInstaller.Internals;

namespace Relativity.IntegrationPoints.SourceProviderInstaller
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
		private const int _SEND_INSTALL_REQUEST_DELAY_BETWEEN_RETRIES_IN_MS = 3000;

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
			IntegrationPointSourceProviderInstallerInternal internalInstaller = CreateIntegrationPointSourceProviderInstallerInternal();

			int workspaceID = Helper.GetActiveCaseID();
			return internalInstaller.Execute(workspaceID, ApplicationArtifactId);
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

		private IntegrationPointSourceProviderInstallerInternal CreateIntegrationPointSourceProviderInstallerInternal()
		{
			ISourceProviderInstaller sourceProviderInstaller = CreateSourceProviderInstaller();
			var internalInstaller = new IntegrationPointSourceProviderInstallerInternal(
				Logger,
				sourceProviderInstaller,
				GetSourceProviders,
				OnRaisePostInstallPreExecuteEvent,
				OnRaisePostInstallPostExecuteEvent
			);
			return internalInstaller;
		}

		internal virtual ISourceProviderInstaller CreateSourceProviderInstaller()
		{
			IServicesMgr servicesManager = Helper.GetServicesManager();
			var retryHelper = new KeplerRequestHelper(
				Logger, 
				servicesManager, 
				_SEND_INSTALL_REQUEST_MAX_RETRIES_NUMBER, 
				_SEND_INSTALL_REQUEST_DELAY_BETWEEN_RETRIES_IN_MS
			);
			return new KeplerSourceProviderInstaller(retryHelper);
		}
	}
}