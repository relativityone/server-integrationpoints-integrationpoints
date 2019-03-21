using kCura.EventHandler;
using kCura.IntegrationPoints.Services;
using Relativity.API;
using System;

namespace kCura.IntegrationPoints.SourceProviderInstaller
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
		/// <summary>
		/// Occurs before the removal of the data source provider.
		/// </summary>
		public event PreUninstallPreExecuteEvent RaisePreUninstallPreExecuteEvent;

		/// <summary>
		/// Occurs after the removal of all the data source providers in the current application.
		/// </summary>
		public event PreUninstallPostExecuteEvent RaisePreUninstallPostExecuteEvent;

		/// <summary>
		/// Creates a new instance of the data source uninstall provider.
		/// </summary>
		protected IntegrationPointSourceProviderUninstaller() // TODO do we need it???
		{
		}

		/// <summary>
		/// Runs when the event handler is called during the removal of the data source provider.
		/// </summary>
		/// <returns>An object of type Response, which frequently contains a message.</returns>
		public sealed override Response Execute()
		{
			bool isSuccess = false;
			Exception ex = null;
			try
			{
				OnRaisePreUninstallPreExecuteEvent();
				UninstallSourceProvider();

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
				OnRaisePreUninstallPostExecuteEvent(isSuccess, ex);
			}
			return new Response
			{
				Success = isSuccess
			};
		}

		private void UninstallSourceProvider()
		{
			var request = new UninstallProviderRequest
			{
				ApplicationID = ApplicationArtifactId,
				WorkspaceID = Helper.GetActiveCaseID()
			};

			using (var providerManager = Helper.GetServicesManager().CreateProxy<IProviderManager>(ExecutionIdentity.CurrentUser))
			{
				providerManager.UninstallProvider(request);
			}
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
	}
}
