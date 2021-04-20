using System.Collections.Generic;
using FluentAssertions;
using kCura.IntegrationPoints.Data.Models;
using kCura.IntegrationPoints.Data.Repositories;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services
{
    public class FakeAuditRepository : IRelativityAuditRepository
    {
        private List<AuditRecord> _auditHistory = new List<AuditRecord>();
        
        public void CreateAuditRecord(int artifactID, AuditElement auditElement)
        {
            _auditHistory.Add(new AuditRecord
            {
                IntegrationPointId = artifactID,
                AuditMessage = auditElement.AuditMessage
            });
        }

        private struct AuditRecord
        {
            public int IntegrationPointId { get; set; }
            public string AuditMessage { get; set; }
        }
    }
}