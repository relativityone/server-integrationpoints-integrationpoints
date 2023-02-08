using System;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.Agent.CustomProvider.DTO
{
    public class CustomProviderJobDetails
    {
        public Guid JobID { get; set; }

        public List<CustomProviderBatch> Batches { get; set; }
    }
}
