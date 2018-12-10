using kCura.IntegrationPoints.Common.Constants;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class AuditRepository : IAuditRepository
	{
		private readonly IAuditService _auditService;
		private readonly IExternalServiceInstrumentationProvider _instrumentationProvider;

		public AuditRepository(IAuditService auditService, IExternalServiceInstrumentationProvider instrumentationProvider)
		{
			_auditService = auditService;
			_instrumentationProvider = instrumentationProvider;
		}

		public bool AuditExport(global::Relativity.API.Foundation.ExportStatistics exportStats)
		{
			IExternalServiceSimpleInstrumentation instrumentation = _instrumentationProvider.CreateSimple(
				ExternalServiceTypes.API_FOUNDATION,
				nameof(IAuditService),
				nameof(IAuditService.CreateAuditForExport));

			return instrumentation.Execute(() => _auditService.CreateAuditForExport(exportStats));
		}
	}
}
