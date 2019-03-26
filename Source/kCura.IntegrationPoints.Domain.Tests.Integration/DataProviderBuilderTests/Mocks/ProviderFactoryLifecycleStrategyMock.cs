using System;
using kCura.IntegrationPoint.Tests.Core.Exceptions;
using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Core.Services.Domain;

namespace kCura.IntegrationPoints.Domain.Tests.Integration.DataProviderBuilderTests.Mocks
{
	internal class ProviderFactoryLifecycleStrategyMock : IProviderFactoryLifecycleStrategy
	{
		private readonly string _assemblyDirectory;

		public ProviderFactoryLifecycleStrategyMock(string assemblyDirectory)
		{
			_assemblyDirectory = assemblyDirectory;
		}

		public IProviderFactory CreateProviderFactory(Guid applicationId)
		{
			AppDomain otherDomain = CreateMockProviderAppDomain();
			return InstantiateTypeInAppDomain<ProviderFactoryMock>(otherDomain);
		}
		
		private AppDomain CreateMockProviderAppDomain()
		{
			var domainSetup = new AppDomainSetup
			{
				ApplicationBase = _assemblyDirectory
			};
			AppDomain otherDomain = AppDomain.CreateDomain(
				friendlyName: "mock provider domain",
				securityInfo: AppDomain.CurrentDomain.Evidence,
				info: domainSetup
			);
			return otherDomain;
		}

		private static T InstantiateTypeInAppDomain<T>(AppDomain otherDomain)
			where T : class
		{
			Type providerFactoryType = typeof(T);

			if (providerFactoryType.FullName == null)
			{
				throw new TestSetupException();
			}

			var providerFactoryInMockProviderDomain = otherDomain.CreateInstanceAndUnwrap(
				assemblyName: providerFactoryType.Assembly.FullName,
				typeName: providerFactoryType.FullName
			) as T;
			return providerFactoryInMockProviderDomain;
		}

		public void OnReleaseProviderFactory(Guid applicationId)
		{

		}
	}
}
