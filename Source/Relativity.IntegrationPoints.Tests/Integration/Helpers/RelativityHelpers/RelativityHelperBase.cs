using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers.RelativityHelpers
{
    public abstract class RelativityHelperBase
    {
        protected RelativityInstanceTest Relativity { get; }

        protected RelativityHelperBase(RelativityInstanceTest relativity)
        {
            Relativity = relativity;
        }
    }
}