using kCura.IntegrationPoints.Config;

namespace kCura.IntegrationPoints.Core.Factories
{
    public interface IConfigFactory
    {
        IConfig Create();
    }
}