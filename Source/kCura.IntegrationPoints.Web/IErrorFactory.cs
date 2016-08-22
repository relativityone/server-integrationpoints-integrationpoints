using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.MicroKernel;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Services;

namespace kCura.IntegrationPoints.Web
{
	public interface IErrorFactory
	{
		IErrorService GetErrorService();
		void Release(IErrorService errorService);
	}

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
