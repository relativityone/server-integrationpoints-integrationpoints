using Castle.Windsor;
using kCura.IntegrationPoints.Core.Services;

namespace kCura.IntegrationPoints.Web
{
	public class ErrorFactory : IErrorFactory
	{
		private readonly IWindsorContainer _kernel;

		public ErrorFactory(IWindsorContainer kernel)
		{
			_kernel = kernel;
		}

		public IErrorService GetErrorService()
		{
			return _kernel.Resolve<ErrorService>();
		}

		public void Release(IErrorService errorService)
		{
			_kernel.Release(errorService);
		}
	}
}