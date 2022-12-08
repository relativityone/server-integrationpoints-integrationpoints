using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation;

namespace kCura.IntegrationPoints.RelativitySync.RipOverride
{
    internal sealed class EmptyAgentValidator : IAgentValidator
    {
        public void Validate(IntegrationPointDto integrationPointDto, int submittedByUserId)
        {
            // Method intentionally left empty.
        }
    }
}
