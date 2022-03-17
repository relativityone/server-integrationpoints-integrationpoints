using kCura.IntegrationPoints.RelativitySync.RipOverride;
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
		private readonly IAPM _apmMetrics;
		private readonly IAPILog _log;

		private readonly ISyncServiceManager _syncServiceManager;

		public SyncOperationsWrapper(IHelper helper, IAPM apmMetrics, IAPILog log)
		{
			_helper = helper;
			_apmMetrics = apmMetrics;
			_log = log;
			_syncServiceManager = new SyncServiceManagerForRip(_helper.GetServicesManager());
		}

		public ISyncJobFactory CreateSyncJobFactory()
		{
			return new SyncJobFactory();
		}

		public IRelativityServices CreateRelativityServices()
		{
			return new RelativityServices(_apmMetrics, _syncServiceManager, 
				ExtensionPointServiceFinder.ServiceUriProvider.AuthenticationUri(), _helper);
		}

		public ISyncLog CreateSyncLog()
		{
			return new SyncLog(_log);
		}

		public async Task PrepareSyncConfigurationForResumeAsync(int workspaceId, int syncConfigurationId)
		{
			await _syncServiceManager.PrepareSyncConfigurationForResumeAsync(
					workspaceId, syncConfigurationId, CreateSyncLog())
				.ConfigureAwait(false);
		}

		public ISyncConfigurationBuilder GetSyncConfigurationBuilder(ISyncContext context)
		{
			return new SyncConfigurationBuilder(context, _syncServiceManager, CreateSyncLog());
		}
	}
}
