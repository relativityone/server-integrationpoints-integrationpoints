using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using Relativity;
using Relativity.Services.Field;
using Relativity.Services.Search;

namespace kCura.IntegrationPoint.Tests.Core
{
    public static class SavedSearch
    {
        private static ITestHelper Helper => new TestHelper();

        public static int CreateSavedSearch(int workspaceID, string name)
        {
            return CreateSavedSearchAsync(workspaceID, name, new List<FieldRef> { new FieldRef("Control Number")}).GetAwaiter().GetResult();
        }

        public static async Task<int> CreateSavedSearchAsync(int workspaceID, string name, IEnumerable<FieldRef> fields)
        {
            var keywordSearch = new KeywordSearch
            {
                ArtifactTypeID = (int)ArtifactType.Document,
                Name = name,
                Fields = fields.ToList()
            };

            using (var proxy = Helper.CreateProxy<IKeywordSearchManager>())
            {
                return await proxy.CreateSingleAsync(workspaceID, keywordSearch).ConfigureAwait(false);
            }
        }

        public static int Create(int workspaceArtifactID, KeywordSearch search)
        {
            using (var proxy = Helper.CreateProxy<IKeywordSearchManager>())
            {
                return proxy.CreateSingleAsync(workspaceArtifactID, search).GetAwaiter().GetResult();
            }
        }

        public static int CreateSearchFolder(int workspaceArtifactID, SearchContainer searchContainer)
        {
            using (var proxy = Helper.CreateProxy<ISearchContainerManager>())
            {
                return proxy.CreateSingleAsync(workspaceArtifactID, searchContainer).GetAwaiter().GetResult();
            }
        }
    }
}
