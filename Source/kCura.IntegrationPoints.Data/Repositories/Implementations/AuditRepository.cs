using Relativity.API;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class AuditRepository : IAuditRepository
	{
		private readonly IAuditService _auditService;

		public AuditRepository(IAuditService auditService)
		{
			_auditService = auditService;
		}

		public bool AuditExport(global::Relativity.API.Foundation.ExportStatistics exportStats)
		{
			return _auditService.CreateAuditForExport(exportStats);
		}
	}
}
