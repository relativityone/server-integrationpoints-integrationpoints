using System;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
    public class TaskParameters
    {
        public Guid BatchInstance { get; set; }

        public object BatchParameters { get; set; }

        public long? BatchStartingIndex { get; set; }
    }
}
