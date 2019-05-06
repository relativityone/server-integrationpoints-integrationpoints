using System;
using System.Threading.Tasks;
using Relativity.Services.ServiceProxy;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Tests.Common
{
	internal sealed class SourceServiceFactoryStub : ISourceServiceFactoryForAdmin, ISourceServiceFactoryForUser
	{
		private readonly ServiceFactory _serviceFactory;

		public SourceServiceFactoryStub(ServiceFactory serviceFactory)
		{
			_serviceFactory = serviceFactory;
		}

		public async Task<T> CreateProxyAsync<T>() where T : IDisposable
		{
			await Task.Yield();

			return _serviceFactory.CreateProxy<T>();
		}
	}
}