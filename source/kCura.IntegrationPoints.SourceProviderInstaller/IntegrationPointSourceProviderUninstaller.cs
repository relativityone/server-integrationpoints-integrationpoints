using System;
using kCura.EventHandler;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.SourceProviderInstaller.Services;

namespace kCura.IntegrationPoints.SourceProviderInstaller
{
	public delegate void PreUninstallPreExecuteEvent();
	public delegate void PreUninstallPostExecuteEvent(bool isUninstalled, Exception ex);

	public abstract class IntegrationPointSourceProviderUninstaller : kCura.EventHandler.PreUninstallEventHandler
	{
		public event PreUninstallPreExecuteEvent RaisePreUninstallPreExecuteEvent;
		public event PreUninstallPostExecuteEvent RaisePreUninstallPostExecuteEvent;

		public IntegrationPointSourceProviderUninstaller()
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
			ImportService.UninstallProvider(base.ApplicationArtifactId);
		}

		protected void OnRaisePreUninstallPreExecuteEvent()
		{
			if (RaisePreUninstallPreExecuteEvent != null)
				RaisePreUninstallPreExecuteEvent();
		}

		protected void OnRaisePreUninstallPostExecuteEvent(bool isUninstalled, Exception ex)
		{
			if (RaisePreUninstallPostExecuteEvent != null)
				RaisePreUninstallPostExecuteEvent(isUninstalled, ex);
		}
	}
}
