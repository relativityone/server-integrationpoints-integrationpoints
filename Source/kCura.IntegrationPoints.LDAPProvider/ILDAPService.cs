using System.Collections.Generic;
using System.DirectoryServices;

namespace kCura.IntegrationPoints.LDAPProvider
{
    public interface ILDAPService
    {
        void InitializeConnection();
        bool IsAuthenticated();
        IEnumerable<SearchResult> FetchItems(int? overrideSizeLimit = null);
        IEnumerable<SearchResult> FetchItems(string filter, int? overrideSizeLimit);
        IEnumerable<SearchResult> FetchItemsUpTheTree(string filter, int? overrideSizeLimit);
        List<string> FetchAllProperties(int? overrideSizeLimit = null);
    }
}
