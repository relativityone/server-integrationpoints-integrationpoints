using Relativity.Sync.Tests.Performance.ARM;
using Relativity.Sync.Tests.Performance.Helpers;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;

namespace Relativity.Sync.Tests.Performance
{
	abstract public class PerformanceTestsBase
	{
		public ApiComponent Component { get; }
		public ARMHelper ARMHelper { get; }
		public AzureStorageHelper StorageHelper { get; }

		public PerformanceTestsBase()
		{
			RelativityFacade.Instance.RelyOn<ApiComponent>();

			Component = RelativityFacade.Instance.GetComponent<ApiComponent>();

			StorageHelper = AzureStorageHelper.CreateFromTestConfig();

			ARMHelper = ARMHelper.CreateInstance();

			ARMHelper.EnableAgents();
		}
	}
}
