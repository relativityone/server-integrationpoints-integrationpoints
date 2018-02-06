using System;

namespace kCura.IntegrationPoints.Domain
{
	public interface IDomainHelper
	{
		void LoadRequiredAssemblies(AppDomain domain);
		T CreateInstance<T>(AppDomain domain, params object[] constructorArgs) where T : class;
		void LoadClientLibraries(AppDomain domain, Guid applicationGuid);
		void ReleaseDomain(AppDomain domain);
		AppDomain CreateNewDomain();

		IDomainManager SetupDomainAndCreateManager(AppDomain domain,
			Guid applicationGuid);
	}
}