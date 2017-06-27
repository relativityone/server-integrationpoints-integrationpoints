using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Agent.Validation
{
    public class IntegrationPointExecutionEmptyValidator : IIntegrationPointExecutionValidator
    {
        public ValidationResult Validate(IntegrationPointModel integrationModel)
        {
            return new ValidationResult();
        }
    }
}
