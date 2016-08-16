using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using kCura.IntegrationPoints.Data.Models;
using Relativity.Core;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{

	public class RelativityAuditRepository : IRelativityAuditRepository
	{
		private readonly BaseServiceContext _context;

		public RelativityAuditRepository(BaseServiceContext context)
		{
			_context = context;
		}

		public void CreateAuditRecord(int artifactId, AuditElement detail)
		{
			string auditDetail = SerializeAudit(detail);
			AuditHelper.CreateAuditRecord(_context, artifactId, (int)AuditAction.Run, auditDetail);
		}

		private string SerializeAudit(AuditElement detail)
		{
			if (detail == null)
			{
				return String.Empty;
			}

			var xmlNamespace = new XmlSerializerNamespaces();
			xmlNamespace.Add(String.Empty, String.Empty);
			XmlSerializer serializer = new XmlSerializer(typeof (AuditElement));
			XmlWriterSettings settings = new XmlWriterSettings
			{
				OmitXmlDeclaration = true
			};

			using (StringWriter stream = new StringWriter())
			{
				using (XmlWriter writer = XmlWriter.Create(stream, settings))
				{
					serializer.Serialize(writer, detail, xmlNamespace);
				}
				return stream.ToString();
			}
		}
	}
}