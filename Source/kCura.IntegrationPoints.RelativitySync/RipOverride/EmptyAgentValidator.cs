using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.RelativitySync.RipOverride
{
    internal sealed class EmptyAgentValidator : IAgentValidator
    {
        public void Validate(IntegrationPoint integrationPointDto, int submittedByUserId)
        {
            // Method intentionally left empty.
        }
    }
}
