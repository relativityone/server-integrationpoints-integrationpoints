using System;
using kCura.EventHandler;
using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.SourceProviderInstaller.Services;

namespace kCura.IntegrationPoints.SourceProviderInstaller
{
	/// <summary>
	/// 
	/// </summary>
	public delegate void PreUninstallPreExecuteEvent();

	/// <summary>
	/// 
	/// </summary>
	/// <param name="isUninstalled"></param>
	/// <param name="ex"></param>
	public delegate void PreUninstallPostExecuteEvent(bool isUninstalled, Exception ex);

	/// <summary>
	/// Provides a means to remove a source provider when the application is uninstalled.
	/// </summary>
	public abstract class IntegrationPointSourceProviderUninstaller : kCura.EventHandler.PreUninstallEventHandler
	{
		/// <summary>
		/// Event that is raised before the source provider is removed.
		/// </summary>
		public event PreUninstallPreExecuteEvent RaisePreUninstallPreExecuteEvent;

		/// <summary>
		/// Event that is raised after all the source providers for the current application are removed.
		/// </summary>
		public event PreUninstallPostExecuteEvent RaisePreUninstallPostExecuteEvent;

		/// <summary>
		/// Creates a new instance of the source uninstall provider.
		/// </summary>
		protected IntegrationPointSourceProviderUninstaller()
		{
		}

		private ICaseServiceContext _caseContext;
		internal ICaseServiceContext CaseServiceContext
		{
			get
			{
				return _caseContext ?? (_caseContext = ServiceContextFactory.CreateCaseServiceContext(base.Helper, base.Helper.GetActiveCaseID()));
			}
			set { _caseContext = value; }
		}

		private IEddsServiceContext _eddsContext;
		internal IEddsServiceContext EddsServiceContext
		{
			get
			{
				return _eddsContext ?? (_eddsContext = ServiceContextFactory.CreateEddsServiceContext(base.Helper));
			}
			set { _eddsContext = value; }
		}

		private IImportService _importService;
		internal IImportService ImportService
		{
			get
			{
				return _importService ?? (_importService = new ImportService(this.CaseServiceContext, this.EddsServiceContext));
			}
			set { _importService = value; }
		}

		public override sealed Response Execute()
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
				throw Utils.GetNonCustomException(e);
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
			ImportService.UninstallProvider(base.ApplicationArtifactId);
		}


		/// <summary>
		/// Raises the RaisePreUninstallPreExecuteEvent
		/// </summary>
		protected void OnRaisePreUninstallPreExecuteEvent()
		{
			if (RaisePreUninstallPreExecuteEvent != null)
				RaisePreUninstallPreExecuteEvent();
		}

		/// <summary>
		/// Raises the RaisePreUninstallPostExecuteEvent
		/// </summary>
		protected void OnRaisePreUninstallPostExecuteEvent(bool isUninstalled, Exception ex)
		{
			if (RaisePreUninstallPostExecuteEvent != null)
				RaisePreUninstallPostExecuteEvent(isUninstalled, ex);
		}
	}
}
