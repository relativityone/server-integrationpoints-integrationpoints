using kCura.IntegrationPoints.Core.Services;

namespace kCura.IntegrationPoints.Web
{
	public interface IErrorFactory
	{
		IErrorService GetErrorService();
		void Release(IErrorService errorService);
	}
}