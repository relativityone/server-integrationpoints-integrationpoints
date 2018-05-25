using kCura.IntegrationPoints.Services.RelativityWebApi;

namespace kCura.IntegrationPoints.Services.Helpers
{
    public interface IRelativityManagerSoapFactory
    {
        RelativityManagerSoap Create(string url);
    }
}