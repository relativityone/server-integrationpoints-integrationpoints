using System;

namespace kCura.IntegrationPoints.Domain
{
	public interface IAppDomainHelper
	{
        void LoadClientLibraries(AppDomain domain, Guid applicationGuid);
		void ReleaseDomain(AppDomain domain);
		AppDomain CreateNewDomain();
        IAppDomainManager SetupDomainAndCreateManager(AppDomain domain,
			Guid applicationGuid);
	}
}