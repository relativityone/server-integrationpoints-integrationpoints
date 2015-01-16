using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using kCura.EventHandler;

namespace kCura.IntegrationPoints.SourceProviderInstaller
{
	public delegate void PreUninstallPreExecuteEvent();
	public delegate void PreUninstallPostExecuteEvent(bool isUninstalled, Exception ex);

	public abstract class IntegrationPointSourceProviderUninstaller : kCura.EventHandler.PreUninstallEventHandler
	{
		public event PreUninstallPreExecuteEvent RaisePreUninstallPreExecuteEvent;
		public event PreUninstallPostExecuteEvent RaisePreUninstallPostExecuteEvent;
		public abstract IDictionary<Guid, SourceProvider> GetSourceProviders();

		private IImportService _importService;
		internal IntegrationPointSourceProviderUninstaller(IImportService importService)
		{
			_importService = importService;
		}

		public IntegrationPointSourceProviderUninstaller()
		{
			//if (_importService == null) _importService = new ImportService();
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
			_importService.UninstallProvider(base.ApplicationArtifactId);
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
