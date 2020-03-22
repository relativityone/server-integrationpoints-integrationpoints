using NUnit.Framework;
using Relativity.Automation.Utility;
using Relativity.Automation.Utility.Api;
using Relativity.Sync.Tests.Performance.ARM;
using Relativity.Sync.Tests.Performance.Helpers;

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
		}
	}
}
