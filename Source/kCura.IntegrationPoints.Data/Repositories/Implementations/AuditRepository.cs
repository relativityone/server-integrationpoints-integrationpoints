using kCura.IntegrationPoints.Common.Constants;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using Relativity.API.Foundation.Repositories;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
    public class AuditRepository : IAuditRepository
    {
        private readonly IExportAuditRepository _exportAuditRepository;
        private readonly IExternalServiceInstrumentationProvider _instrumentationProvider;

        public AuditRepository(IExportAuditRepository exportAuditRepository, IExternalServiceInstrumentationProvider instrumentationProvider)
        {
            _exportAuditRepository = exportAuditRepository;
            _instrumentationProvider = instrumentationProvider;
        }

        public bool AuditExport(global::Relativity.API.Foundation.ExportStatistics exportStats, int contextUserId)
        {
            IExternalServiceSimpleInstrumentation instrumentation = _instrumentationProvider.CreateSimple(
                ExternalServiceTypes.API_FOUNDATION,
                nameof(IExportAuditRepository),
                nameof(IExportAuditRepository.CreateAuditForExport));

            return instrumentation.Execute(() => _exportAuditRepository.CreateAuditForExport(exportStats, contextUserId));
        }
    }
}
