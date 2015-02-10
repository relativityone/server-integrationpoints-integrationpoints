using System;
using System.Collections.Generic;
using System.Linq;
using kCura.EventHandler;
using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.SourceProviderInstaller.Services;

namespace kCura.IntegrationPoints.SourceProviderInstaller
{
	public delegate void PostInstallPreExecuteEvent();
	public delegate void PostInstallPostExecuteEvent(bool isInstalled, Exception ex);

	public abstract class IntegrationPointSourceProviderInstaller : kCura.EventHandler.PostInstallEventHandler
	{
		public event PostInstallPreExecuteEvent RaisePostInstallPreExecuteEvent;
		public event PostInstallPostExecuteEvent RaisePostInstallPostExecuteEvent;
		public abstract IDictionary<Guid, SourceProvider> GetSourceProviders();

		public IntegrationPointSourceProviderInstaller()
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

		protected void OnRaisePostInstallPreExecuteEvent()
		{
			if (RaisePostInstallPreExecuteEvent != null)
				RaisePostInstallPreExecuteEvent();
		}

		protected void OnRaisePostInstallPostExecuteEvent(bool isInstalled, Exception ex)
		{
			if (RaisePostInstallPostExecuteEvent != null)
				RaisePostInstallPostExecuteEvent(isInstalled, ex);
		}
	}
}
