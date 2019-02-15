namespace kCura.IntegrationPoints.Web.Logging
{
	public interface IWebCorrelationContextProvider
	{
		WebActionContext GetDetails(string url, int userId);
	}
}
