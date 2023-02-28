

namespace kCura.IntegrationPoints.Web.IntegrationPointsServices.Logging
{
    internal interface ICacheHolder
    {
        T GetObject<T>(string key) where T : class;

        void SetObject<T>(string key, T value) where T : class;
    }
}
