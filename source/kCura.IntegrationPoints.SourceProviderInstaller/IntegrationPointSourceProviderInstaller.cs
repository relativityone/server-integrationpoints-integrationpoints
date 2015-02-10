using System;
using System.Collections.Generic;
using System.Linq;
using kCura.EventHandler;
using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.SourceProviderInstaller.Services;

namespace kCura.IntegrationPoints.SourceProviderInstaller
{
	/// <summary>
	/// Event Executed before post install will begin.
	/// </summary>
	public delegate void PostInstallPreExecuteEvent();
	/// <summary>
	/// Event executed after all source providers are registered
	/// </summary>
	/// <param name="isInstalled">Whether the source providers were installed.</param>
	/// <param name="ex">The exception that occured if there were errors in installing the source provider.</param>
	public delegate void PostInstallPostExecuteEvent(bool isInstalled, Exception ex);
	
	/// <summary>
	/// Provides a means to register new source providers with Relativity Integration points.
	/// </summary>
	public abstract class IntegrationPointSourceProviderInstaller : kCura.EventHandler.PostInstallEventHandler
	{
		public event PostInstallPreExecuteEvent RaisePostInstallPreExecuteEvent;
		public event PostInstallPostExecuteEvent RaisePostInstallPostExecuteEvent;

		/// <summary>
		/// Gets all the source providers that will be registered with this application.
		/// </summary>
		/// <returns>Source providers that are expected to be registered.</returns>
		public abstract IDictionary<Guid, SourceProvider> GetSourceProviders();

		protected IntegrationPointSourceProviderInstaller()
		{}

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
				Name = x.Value.Name,
				Url = x.Value.Url
			}).ToList();

			ImportService.InstallProviders(sourceProviders);
		}

		/// <summary>
		/// Raise the event for post install pre execute.
		/// </summary>
		protected void OnRaisePostInstallPreExecuteEvent()
		{
			if (RaisePostInstallPreExecuteEvent != null)
				RaisePostInstallPreExecuteEvent();
		}

		/// <summary>
		/// Raise event after the registration process was completed.
		/// </summary>
		/// <param name="isInstalled">Whether the source providers were installed.</param>
		/// <param name="ex">The exception that occured if there were errors in installing the source provider.</param>
		protected void OnRaisePostInstallPostExecuteEvent(bool isInstalled, Exception ex)
		{
			if (RaisePostInstallPostExecuteEvent != null)
				RaisePostInstallPostExecuteEvent(isInstalled, ex);
		}
	}
}
