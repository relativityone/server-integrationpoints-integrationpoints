using System;
using kCura.EventHandler;
using Relativity.API;
using Relativity.IntegrationPoints.SourceProviderInstaller.Internals;

namespace Relativity.IntegrationPoints.SourceProviderInstaller
{
	/// <summary>
	/// Occurs immediately before the execution of a Pre Uninstall event handler.
	/// </summary>
	public delegate void PreUninstallPreExecuteEvent();

	/// <summary>
	/// Occurs after the source provider is removed from the database table.
	/// </summary>
	/// <param name="isUninstalled"></param>
	/// <param name="ex"></param>
	public delegate void PreUninstallPostExecuteEvent(bool isUninstalled, Exception ex);

	/// <summary>
	/// Removes a data source provider when the user uninstalls the application from a workspace.
	/// </summary>
	public abstract class IntegrationPointSourceProviderUninstaller : PreUninstallEventHandler
	{
		private const int _SEND_INSTALL_REQUEST_MAX_RETRIES_NUMBER = 3;
		private const int _SEND_INSTALL_REQUEST_DELAY_BETWEEN_RETRIES_IN_MS = 3000;


		private readonly Lazy<IAPILog> _logggerLazy;

		/// <summary>
		/// Occurs before the removal of the data source provider.
		/// </summary>
		public event PreUninstallPreExecuteEvent RaisePreUninstallPreExecuteEvent;

		/// <summary>
		/// Occurs after the removal of all the data source providers in the current application.
		/// </summary>
		public event PreUninstallPostExecuteEvent RaisePreUninstallPostExecuteEvent;

		private IAPILog Logger => _logggerLazy.Value;

		/// <summary>
		/// Creates a new instance of the data source uninstall provider.
		/// </summary>
		protected IntegrationPointSourceProviderUninstaller()
		{
			_logggerLazy = new Lazy<IAPILog>(
				() => Helper.GetLoggerFactory().GetLogger().ForContext<IntegrationPointSourceProviderInstaller>()
			);
		}

		/// <summary>
		/// Runs when the event handler is called during the removal of the data source provider.
		/// </summary>
		/// <returns>An object of type Response, which frequently contains a message.</returns>
		public sealed override Response Execute()
		{
			IKeplerRequestHelper keplerRequestHelper = CreateKeplerRequestHelper();
			var internalUninstaller = new IntegrationPointSourceProviderUninstallerInternal(
				keplerRequestHelper,
				OnRaisePreUninstallPreExecuteEvent,
				OnRaisePreUninstallPostExecuteEvent
			);

			int workspaceID = Helper.GetActiveCaseID();
			return internalUninstaller.Execute(workspaceID, ApplicationArtifactId);
		}

		/// <summary>
		/// Raises the RaisePreUninstallPreExecuteEvent.
		/// </summary>
		protected void OnRaisePreUninstallPreExecuteEvent()
		{
			RaisePreUninstallPreExecuteEvent?.Invoke();
		}

		/// <summary>
		/// Raises the RaisePreUninstallPostExecuteEvent.
		/// </summary>
		protected void OnRaisePreUninstallPostExecuteEvent(bool isUninstalled, Exception ex)
		{
			RaisePreUninstallPostExecuteEvent?.Invoke(isUninstalled, ex);
		}

		internal virtual IKeplerRequestHelper CreateKeplerRequestHelper()
		{
			IServicesMgr servicesManager = Helper.GetServicesManager();
			return new KeplerRequestHelper(
				Logger,
				servicesManager,
				_SEND_INSTALL_REQUEST_MAX_RETRIES_NUMBER,
				_SEND_INSTALL_REQUEST_DELAY_BETWEEN_RETRIES_IN_MS
			);
		}
	}
}
