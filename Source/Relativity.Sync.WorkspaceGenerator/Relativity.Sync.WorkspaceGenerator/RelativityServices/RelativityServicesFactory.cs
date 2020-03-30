using System;
using Relativity.Services.ServiceProxy;

namespace Relativity.Sync.WorkspaceGenerator.RelativityServices
{
	public class RelativityServicesFactory
	{
		private readonly ServiceFactory _serviceFactory;

		public RelativityServicesFactory(GeneratorSettings settings)
		{
			var credentials = new UsernamePasswordCredentials(settings.RelativityUserName, settings.RelativityPassword);
			var serviceFactorySettings = new ServiceFactorySettings(settings.RelativityServicesUri, settings.RelativityRestApiUri, credentials);
			_serviceFactory = new ServiceFactory(serviceFactorySettings);
		}

		public WorkspaceService CreateWorkspaceService()
		{
			return new WorkspaceService(_serviceFactory);
		}
	}
}