using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using kCura.EventHandler;
using kCura.IntegrationPoints.Core.Extensions;
using kCura.IntegrationPoints.Core.Logging;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.SourceProviderInstaller.Services;
using Relativity.API;

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
	public abstract class IntegrationPointSourceProviderInstaller : PostInstallEventHandlerBase
	{
		/// <summary>
		/// It returns message that will be generated on sucessfull installation
		/// </summary>
		protected override string SuccessMessage => "Created or updated successfully.";

		/// <summary>
		/// It returns message that will be generated on Error Tab and log on installation error
		/// </summary>
		/// <param name="ex"></param>
		/// <returns></returns>
		protected override string GetFailureMessage(Exception ex)
		{
			IDictionary<Guid, SourceProvider> sourceProviders = GetSourceProviders();
			var failureMessage = new StringBuilder("Failed to install");

			foreach (var sourceProv in sourceProviders)
			{
				failureMessage.Append(" [Provider: ");
				failureMessage.Append(sourceProv.Value?.Name);
				failureMessage.Append("]");
			}
			return failureMessage.ToString();
		}

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

		internal DeleteHistoryService DeleteHistory
		{
			get { return _deleteHistoryService ?? (_deleteHistoryService = new DeleteHistoryService(new RSAPIServiceFactory(Helper))); }
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
		protected override void Run()
		{
			IDictionary<Guid, SourceProvider> sourceProviders = GetSourceProviders();
			Logger.LogDebug("Starting Post-installation process for {sourceProviders} provider", sourceProviders.Values.Select(item => item.Name));
			if (sourceProviders.Count == 0)
			{
				throw new IntegrationPointsException($"Provider does not implement the contract (Empty source provider list retrieved from {GetType().Name} class)");
			}
			OnRaisePostInstallPreExecuteEvent();
			InstallSourceProvider(GetSourceProviders());
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
		protected override void OnRaisePostInstallPreExecuteEvent()
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
