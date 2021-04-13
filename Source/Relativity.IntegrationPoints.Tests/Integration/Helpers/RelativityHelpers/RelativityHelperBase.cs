namespace Relativity.IntegrationPoints.Tests.Integration.Helpers.RelativityHelpers
{
    public abstract class RelativityHelperBase
    {
        public RelativityInstanceTest Relativity { get; }
        public RelativityHelperBase(RelativityInstanceTest relativity)
        {
            Relativity = relativity;
        }
    }
}