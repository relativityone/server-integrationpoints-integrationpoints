using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using Relativity.Productions.Services;

namespace kCura.IntegrationPoint.Tests.Core
{
    public static class Placeholder
    {
        private static ITestHelper Helper => new TestHelper();

        public static int Create(int workspaceId, byte[] fileData)
        {
            var productionPlaceholder = new ProductionPlaceholder
            {
                PlaceholderType = PlaceholderType.Image,
                FileData = fileData,
                Filename = "DefaultPlaceholder.tif",
                Name = "CustomPlaceholder"
            };

            using (var proxy = Helper.CreateProxy<IProductionPlaceholderManager>())
            {
                return proxy.CreateSingleAsync(workspaceId, productionPlaceholder).GetAwaiter().GetResult();
            }
        }
    }
}
