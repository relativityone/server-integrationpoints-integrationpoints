using System;
using System.Xml.Linq;
using System.Xml.Serialization;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Common.Constants;
using kCura.IntegrationPoints.Common.Handlers;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Data.Models;
using kCura.IntegrationPoints.Data.Repositories.DTO;
using Relativity.API.Foundation;
using IFoundationAuditRepository = Relativity.API.Foundation.Repositories.IAuditRepository;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
    public class RelativityAuditRepository : IRelativityAuditRepository
    {
        private const ushort _MAX_NUMBER_OF_RETRIES = 3;
        private const ushort _EXPONENTIAL_WAIT_TIME_BASE_IN_SECONDS = 3;

        private readonly IFoundationAuditRepository _foundationAuditRepository;
        private readonly IExternalServiceInstrumentationProvider _instrumentationProvider;
        private readonly IRetryHandler _retryHandler;

        public RelativityAuditRepository(
            IFoundationAuditRepository foundationAuditRepository, 
            IExternalServiceInstrumentationProvider instrumentationProvider,
            IRetryHandlerFactory retryHandlerFactory)
        {
            _foundationAuditRepository = foundationAuditRepository;
            _instrumentationProvider = instrumentationProvider;
            _retryHandler = retryHandlerFactory.Create(_MAX_NUMBER_OF_RETRIES, _EXPONENTIAL_WAIT_TIME_BASE_IN_SECONDS);
        }

        public void CreateAuditRecord(int artifactID, AuditElement auditElement)
        {
            XElement detailsXElement = ConvertAuditElementToXElement(auditElement);
            IAuditRecord auditRecord =
                new AuditRecord(artifactID, AuditAction.Run, detailsXElement, executionTime: TimeSpan.Zero);

            IExternalServiceSimpleInstrumentation instrumentation = _instrumentationProvider.CreateSimple(
                ExternalServiceTypes.API_FOUNDATION,
                nameof(global::Relativity.API.Foundation.Repositories.IAuditRepository),
                nameof(IFoundationAuditRepository.CreateAuditRecord));

            _retryHandler.ExecuteWithRetries(
                () => instrumentation.Execute(
                    () => _foundationAuditRepository.CreateAuditRecord(auditRecord)));
        }

        private static XElement ConvertAuditElementToXElement(AuditElement auditElement)
        {
            var xDocument = new XDocument();

            if (auditElement == null)
            {
                return xDocument.Root;
            }

            var xmlSerializerNamespaces = new XmlSerializerNamespaces();
            xmlSerializerNamespaces.Add(prefix: string.Empty, ns: string.Empty);
            var xmlSerializer = new XmlSerializer(typeof(AuditElement));
            using (var writer = xDocument.CreateWriter())
            {
                xmlSerializer.Serialize(writer, auditElement, xmlSerializerNamespaces);
            }

            return xDocument.Root;
        }
    }
}
