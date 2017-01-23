using System;
using kCura.EventHandler;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.SourceProviderInstaller.Services;

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
	public abstract class IntegrationPointSourceProviderUninstaller : kCura.EventHandler.PreUninstallEventHandler
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
		protected IntegrationPointSourceProviderUninstaller()
		{
		}

		private IWorkspaceDBContext _workspaceDbContext;
		internal IWorkspaceDBContext GetWorkspaceDbContext()
		{
			return _workspaceDbContext ??
						 (_workspaceDbContext = new WorkspaceContext(base.Helper.GetDBContext(base.Helper.GetActiveCaseID())));
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
		private IntegrationPointQuery _integrationPointQuery;
		private DeleteHistoryService _deleteHistoryService;
		private IRSAPIService _service;
		private DeleteHistoryErrorService deleteHistoryErrorService;
		internal DeleteHistoryErrorService DeleteHistoryErrorService
		{
			get
			{
				return deleteHistoryErrorService ??
							 (deleteHistoryErrorService =
								 new DeleteHistoryErrorService(Service));
			}
			set { deleteHistoryErrorService = value; }
		}

		internal IntegrationPointQuery IntegrationPoint
		{
			get
			{
				return _integrationPointQuery ?? (_integrationPointQuery = new IntegrationPointQuery(Service));
			}
		}

		internal DeleteHistoryService DeleteHistory
		{
			get { return _deleteHistoryService ?? (_deleteHistoryService = new DeleteHistoryService(Service, DeleteHistoryErrorService)); }
		}

		internal IRSAPIService Service
		{

			get
			{
				if (_service == null)
				{
					_service = new RSAPIService(base.Helper, base.Helper.GetActiveCaseID());
				}
				return _service;
			}
		}

		private DeleteIntegrationPoints _deleteIntegrationPoints;
		internal DeleteIntegrationPoints DeleteIntegrationPoints
		{
			get
			{
				return _deleteIntegrationPoints ?? (_deleteIntegrationPoints = new DeleteIntegrationPoints(IntegrationPoint, DeleteHistory, Service));
			}
			set { _deleteIntegrationPoints = value; }
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
				if (_importService == null)
				{
					_importService = new ImportService(this.CaseServiceContext, DeleteIntegrationPoints, base.Helper);
				}

				return _importService;
			}
			set { _importService = value; }
		}

		/// <summary>
		/// Runs when the event handler is called during the removal of the data source provider.
		/// </summary>
		/// <returns>An object of type Response, which frequently contains a message.</returns>
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
		/// Raises the RaisePreUninstallPreExecuteEvent.
		/// </summary>
		protected void OnRaisePreUninstallPreExecuteEvent()
		{
			if (RaisePreUninstallPreExecuteEvent != null)
				RaisePreUninstallPreExecuteEvent();
		}

		/// <summary>
		/// Raises the RaisePreUninstallPostExecuteEvent.
		/// </summary>
		protected void OnRaisePreUninstallPostExecuteEvent(bool isUninstalled, Exception ex)
		{
			if (RaisePreUninstallPostExecuteEvent != null)
				RaisePreUninstallPostExecuteEvent(isUninstalled, ex);
		}
	}
}
