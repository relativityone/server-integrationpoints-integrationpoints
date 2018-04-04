using System;
using kCura.WinEDDS.Service.Export;

namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
	public abstract class FileServiceBase
	{
		protected readonly Lazy<ISearchManager> SearchManagerLazy;
		protected ISearchManager SearchManager => SearchManagerLazy.Value;

		protected FileServiceBase(ITestHelper testHelper)
		{
			SearchManagerLazy = new Lazy<ISearchManager>(testHelper.CreateSearchManager);
		}
	}
}