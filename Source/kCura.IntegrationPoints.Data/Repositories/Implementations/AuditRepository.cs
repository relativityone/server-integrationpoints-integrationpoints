using Relativity.API;
using Relativity.APIHelper.Audit;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class AuditRepository : IAuditRepository
	{
		public bool AuditExport(int appID, global::Relativity.API.Foundation.ExportStatistics exportStats)
		{
			var auditServiceFactory = new AuditServiceFactory();
			IAuditService auditService = auditServiceFactory.GetAuditService(appID);
			return auditService.CreateAuditForExport(exportStats);
		}
	}
}
