using System.Diagnostics;
using System.Reflection;

namespace kCura.IntegrationPoints.Common.Monitoring
{
    public interface IRipAppVersionProvider
    {
        string Get();
    }

    public class RipAppVersionProvider : IRipAppVersionProvider
    {
        public string Get() => FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
    }
}