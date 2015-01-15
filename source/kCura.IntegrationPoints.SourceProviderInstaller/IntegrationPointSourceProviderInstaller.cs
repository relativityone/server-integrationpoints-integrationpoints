using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using kCura.EventHandler;

namespace kCura.IntegrationPoints.SourceProviderInstaller
{
	public delegate void PostInstallPreExecuteEvent();

	public delegate void PostInstallPostExecuteEvent(bool isInstalled, Exception ex);

	public abstract class IntegrationPointSourceProviderInstaller : kCura.EventHandler.PostInstallEventHandler
	{
		public event PostInstallPreExecuteEvent RaisePostInstallPreExecuteEvent;
		public event PostInstallPostExecuteEvent RaisePostInstallPostExecuteEvent;
		public abstract IDictionary<Guid, SourceProvider> GetSourceProviders();

		private IImportService _importService;
		internal IntegrationPointSourceProviderInstaller(IImportService importService)
		{
			_importService = importService;
		}

		public IntegrationPointSourceProviderInstaller()
		{
			if (_importService == null) _importService = new ImportService();
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
				throw;
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
			List<SourceProvider> sourceProviders = providers.Select(x => new SourceProvider()
			{
				GUID = x.Key,
				Name = x.Value.Name,
				FileLocation = x.Value.FileLocation
			}).ToList();

			foreach (SourceProvider provider in sourceProviders)
			{
				_importService.InstallProvider(provider);
			}
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
