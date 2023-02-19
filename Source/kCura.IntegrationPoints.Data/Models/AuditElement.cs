using System.Xml.Serialization;

namespace kCura.IntegrationPoints.Data.Models
{
    [XmlRoot("auditElement")]
    public class AuditElement
    {
        [XmlElement("auditMessage")]
        public string AuditMessage { get; set; }
    }
}
