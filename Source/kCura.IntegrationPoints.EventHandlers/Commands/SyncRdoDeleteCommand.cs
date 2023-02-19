using kCura.IntegrationPoints.EventHandlers.Commands.Context;
using kCura.IntegrationPoints.RelativitySync.RdoCleanup;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
    public class SyncRdoDeleteCommand : IEHCommand
    {
        private readonly IEHContext _context;
        private readonly ISyncRdoCleanupService _syncRdoCleanupService;

        public SyncRdoDeleteCommand(IEHContext context, ISyncRdoCleanupService syncRdoCleanupService)
        {
            _context = context;
            _syncRdoCleanupService = syncRdoCleanupService;
        }

        public void Execute()
        {
            _syncRdoCleanupService.DeleteSyncRdosAsync(_context.Helper.GetActiveCaseID()).GetAwaiter().GetResult();
        }
    }
}
