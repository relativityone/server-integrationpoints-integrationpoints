

namespace kCura.IntegrationPoints.Web.Logging
{
	internal interface ICacheHolder
	{
		T GetObject<T>(string key) where T : class;
		void SetObject<T>(string key, T value) where T : class;
	}
}
