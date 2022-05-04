namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
	public abstract class FileServiceBase
	{
        protected readonly ITestHelper TestHelper;

        protected FileServiceBase(ITestHelper testHelper)
        {
            TestHelper = testHelper;
        }
	}
}