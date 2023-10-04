using System;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.Agent.CustomProvider.DTO
{
    public class CustomProviderJobDetails
    {
        public Guid BatchInstance { get; set; }

        public int JobHistoryID { get; set; }

        public Guid JobHistoryGuid { get; set; }

        public List<CustomProviderBatch> Batches { get; set; } = new List<CustomProviderBatch>();
    }
}
