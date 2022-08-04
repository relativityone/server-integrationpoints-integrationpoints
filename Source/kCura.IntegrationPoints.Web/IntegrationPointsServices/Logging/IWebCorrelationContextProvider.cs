namespace kCura.IntegrationPoints.Web.IntegrationPointsServices.Logging
{
    public interface IWebCorrelationContextProvider
    {
        WebActionContext GetDetails(string url, int userId);
    }
}
