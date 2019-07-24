using System;
using System.Xml.Linq;
using System.Xml.Serialization;
using kCura.IntegrationPoints.Common.Constants;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Data.Models;
using kCura.IntegrationPoints.Data.Repositories.DTO;
using Relativity.API.Foundation;
using IFoundationAuditRepository = Relativity.API.Foundation.Repositories.IAuditRepository;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class RelativityAuditRepository : IRelativityAuditRepository
	{
		private readonly IFoundationAuditRepository _foundationAuditRepository;
		private readonly IExternalServiceInstrumentationProvider _instrumentationProvider;

		public RelativityAuditRepository(
			IFoundationAuditRepository foundationAuditRepository, 
			IExternalServiceInstrumentationProvider instrumentationProvider)
		{
			_foundationAuditRepository = foundationAuditRepository;
			_instrumentationProvider = instrumentationProvider;
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

			instrumentation.Execute(() => _foundationAuditRepository.CreateAuditRecord(auditRecord));
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
