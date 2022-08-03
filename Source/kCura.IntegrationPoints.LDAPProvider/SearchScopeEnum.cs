using System.DirectoryServices;

namespace kCura.IntegrationPoints.LDAPProvider
{
    public enum SearchScopeEnum
    {
        Base = SearchScope.Base,
        OneLevel = SearchScope.OneLevel,
        Subtree = SearchScope.Subtree
    }
}
