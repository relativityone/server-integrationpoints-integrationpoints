using System;

namespace kCura.IntegrationPoints.Domain
{
	public interface IAppDomainHelper
	{
        T CreateInstance<T>(AppDomain domain, params object[] constructorArgs) where T : class;
		void LoadClientLibraries(AppDomain domain, Guid applicationGuid);
		void ReleaseDomain(AppDomain domain);
		AppDomain CreateNewDomain();

		IAppDomainManager SetupDomainAndCreateManager(AppDomain domain,
			Guid applicationGuid);
	}
}