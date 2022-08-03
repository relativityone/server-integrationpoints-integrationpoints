using System;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.Domain.Logging
{
    public class WebCorrelationContext : BaseCorrelationContext
    {
        public Guid? CorrelationId { get; set; }
        public Guid? WebRequestCorrelationId { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> baseProperties = base.ToDictionary();
            baseProperties.Add(nameof(CorrelationId), CorrelationId);
            baseProperties.Add(nameof(WebRequestCorrelationId), WebRequestCorrelationId);
            return baseProperties;
        }

        public override void SetValuesFromDictionary(Dictionary<string, object> dictionary)
        {
            if (dictionary == null)
            {
                return;
            }
            base.SetValuesFromDictionary(dictionary);
            CorrelationId = GetValueOrDefault<Guid?>(dictionary, nameof(CorrelationId));
            WebRequestCorrelationId = GetValueOrDefault<Guid?> (dictionary, nameof(WebRequestCorrelationId));
        }
    }
}
