using System;
using System.Collections.Generic;
using System.Linq;
using kCura.EventHandler;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.SourceProviderInstaller.Services;

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
	public abstract class IntegrationPointSourceProviderInstaller : kCura.EventHandler.PostInstallEventHandler
	{
		/// <summary>
		/// Raised immediately before the execution of a Post Install event handler.
		/// </summary>
		public event PostInstallPreExecuteEvent RaisePostInstallPreExecuteEvent;
		/// <summary>
		/// Raised after all source providers are registered.
		/// </summary>
		public event PostInstallPostExecuteEvent RaisePostInstallPostExecuteEvent;

		/// <summary>
		/// Retrieves the data source providers for registration with the application.
		/// </summary>
		/// <returns>The data source providers for registration.</returns>
		public abstract IDictionary<Guid, SourceProvider> GetSourceProviders();

		/// <summary>
		/// Default constructor
		/// </summary>
		protected IntegrationPointSourceProviderInstaller()
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
		private IntegrationPointQuery _integrationPointQuery;
		private DeleteHistoryService _deleteHistoryService;
		private IRSAPIService _service;

		internal IntegrationPointQuery IntegrationPoint
		{
			get
			{
				return _integrationPointQuery ?? (_integrationPointQuery = new IntegrationPointQuery(Service));
			}
		}
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
		internal DeleteHistoryService DeleteHistory
		{
			get { return _deleteHistoryService ?? (_deleteHistoryService = new DeleteHistoryService(Service,DeleteHistoryErrorService)); }
		}

		internal IRSAPIService Service
		{
			get { return _service ?? (_service = new RSAPIService(Helper, Helper.GetActiveCaseID())); }
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
		private IImportService _importService;
		internal IImportService ImportService
		{
			get
			{
				if (_importService == null)
				{
					_importService = new ImportService(this.CaseServiceContext, this.EddsServiceContext, DeleteIntegrationPoints, base.Helper);
				}

				return _importService;
			}
			set { _importService = value; }
		}
		/// <summary>
		/// Runs when the event handler is called during the installation of the data source provider.
		/// </summary>
		/// <returns>An object of type Response, which frequently contains a message.</returns>
		public override sealed Response Execute()
		{
			bool isSuccess = false;
			Exception ex = null;
			try
			{
				OnRaisePostInstallPreExecuteEvent();
				InstallSourceProvider(GetSourceProviders());
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

			List<SourceProvider> sourceProviders = providers.Select(x => new SourceProvider()
			{
				GUID = x.Key,
				ApplicationID = base.ApplicationArtifactId,
				ApplicationGUID = x.Value.ApplicationGUID,
				Name = x.Value.Name,
				Url = x.Value.Url,
				ViewDataUrl = x.Value.ViewDataUrl,
				Configuration = x.Value.Configuration
			}).ToList();

			ImportService.InstallProviders(sourceProviders);
		}

		/// <summary>
		/// Raises an event prior to the execution of a Post Install event handler.
		/// </summary>
		protected void OnRaisePostInstallPreExecuteEvent()
		{
			if (RaisePostInstallPreExecuteEvent != null)
				RaisePostInstallPreExecuteEvent();
		}

		/// <summary>
		/// Occurs after the registration process completes.
		/// </summary>
		/// <param name="isInstalled">Indicates whether the data source providers were installed.</param>
		/// <param name="ex">An exception thrown when errors occur during the installation of the data source provider.</param>
		protected void OnRaisePostInstallPostExecuteEvent(bool isInstalled, Exception ex)
		{
			if (RaisePostInstallPostExecuteEvent != null)
				RaisePostInstallPostExecuteEvent(isInstalled, ex);
		}
	}
}
