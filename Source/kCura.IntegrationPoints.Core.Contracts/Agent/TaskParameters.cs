using System;

namespace kCura.IntegrationPoints.Core.Contracts.Agent
{
    public class TaskParameters
    {
        public Guid BatchInstance { get; set; }

        public object BatchParameters{ get; set; }
    }
}
