using Relativity.IntegrationPoints.Services.RelativityWebApi;

namespace Relativity.IntegrationPoints.Services.Helpers
{
    public interface IRelativityManagerSoapFactory
    {
        RelativityManagerSoap Create(string url);
    }
}