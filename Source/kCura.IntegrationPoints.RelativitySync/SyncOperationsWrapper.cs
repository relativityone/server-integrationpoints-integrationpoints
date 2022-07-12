using Relativity.API;
using Relativity.Sync;
using Relativity.Sync.Extensions;
using Relativity.Sync.SyncConfiguration;
using Relativity.Telemetry.APM;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.RelativitySync
{
	public class SyncOperationsWrapper : ISyncOperationsWrapper
	{
		private readonly IHelper _helper;
        private readonly IServicesMgr _servicesMgr;
		private readonly IAPM _apmMetrics;
		private readonly IAPILog _log;
		
		public SyncOperationsWrapper(IHelper helper, IAPM apmMetrics, IAPILog log)
		{
			_helper = helper;
            _servicesMgr = helper.GetServicesManager();
			_apmMetrics = apmMetrics;
			_log = log;
		}

		public ISyncJobFactory CreateSyncJobFactory()
		{
			return new SyncJobFactory();
		}

		public IRelativityServices CreateRelativityServices()
		{
			return new RelativityServices(_apmMetrics, ExtensionPointServiceFinder.ServiceUriProvider.AuthenticationUri(), _helper);
		}
		
		public async Task PrepareSyncConfigurationForResumeAsync(int workspaceId, int syncConfigurationId)
		{
			await _servicesMgr.PrepareSyncConfigurationForResumeAsync(workspaceId, syncConfigurationId, _log).ConfigureAwait(false);
		}

		public ISyncConfigurationBuilder GetSyncConfigurationBuilder(ISyncContext context)
		{
            return new SyncConfigurationBuilder(context, _servicesMgr, _log);
		}
	}
}
