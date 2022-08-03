using kCura.EDDS.WebAPI.AuditManagerBase;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.WinEDDS.Service.Export;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers
{
    public class CoreAuditManager : IAuditManager
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly CurrentUser _currentUser;

        public CoreAuditManager(IRepositoryFactory repositoryFactory, CurrentUser currentUser)
        {
            _repositoryFactory = repositoryFactory;
            _currentUser = currentUser;
        }

        public bool AuditExport(int appID, bool isFatalError, ExportStatistics exportStats)
        {
            IAuditRepository auditRepository = _repositoryFactory.GetAuditRepository(appID);
            return auditRepository.AuditExport(exportStats.ToFoundationExportStatistics(), _currentUser.ID);
        }
    }
}
