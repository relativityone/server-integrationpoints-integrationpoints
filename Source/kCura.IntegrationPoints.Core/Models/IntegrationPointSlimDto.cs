using System;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.Core.Models
{
    public class IntegrationPointSlimDto : IntegrationPointSlimDtoBase
    {
        public DateTime? LastRun { get; set; }

        public bool? HasErrors { get; set; }

        public List<int> JobHistory { get; set; }
    }
}
