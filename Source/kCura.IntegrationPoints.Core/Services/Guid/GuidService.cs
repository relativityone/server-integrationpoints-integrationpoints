namespace kCura.IntegrationPoints.Core.Services
{
    public class DefaultGuidService : IGuidService
    {
        public System.Guid NewGuid()
        {
            return System.Guid.NewGuid();
        }
    }
}
