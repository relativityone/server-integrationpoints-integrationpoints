using System.Collections.Generic;

namespace kCura.IntegrationPoints.Domain.Logging
{
    public class AgentCorrelationContext : BaseCorrelationContext
    {
        public long JobId { get; set; }
        public long? RootJobId { get; set; }
        public long? IntegrationPointId { get; set; }
        public string WorkflowId { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> baseProperties = base.ToDictionary();
            baseProperties.Add(nameof(JobId), JobId);
            baseProperties.Add(nameof(RootJobId), RootJobId);
            baseProperties.Add(nameof(IntegrationPointId), IntegrationPointId);
            baseProperties.Add(nameof(WorkflowId), WorkflowId);
            return baseProperties;
        }

        public override void SetValuesFromDictionary(Dictionary<string, object> dictionary)
        {
            if (dictionary == null)
            {
                return;
            }
            base.SetValuesFromDictionary(dictionary);
            JobId = GetValueOrDefault<long>(dictionary, nameof(JobId));
            RootJobId = GetValueOrDefault<long?>(dictionary, nameof(RootJobId));
            IntegrationPointId = GetValueOrDefault<long?>(dictionary, nameof(IntegrationPointId));
            WorkflowId = GetValueOrDefault<string>(dictionary, nameof(WorkflowId));
        }
    }
}
