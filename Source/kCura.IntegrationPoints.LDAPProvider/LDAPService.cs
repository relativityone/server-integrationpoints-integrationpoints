using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Runtime.InteropServices;
using kCura.Apps.Common.Utils.Serializers;
using Relativity.API;

namespace kCura.IntegrationPoints.LDAPProvider
{
    public class LDAPService : ILDAPService, IDisposable
    {
        private DirectoryEntry _searchRoot;
        private readonly LDAPSettings _settings;
        private readonly LDAPSecuredConfiguration _securedConfiguration;
        private readonly List<string> _fieldsToLoad;
        private readonly IAPILog _logger;

        public LDAPService(IAPILog logger, ISerializer serializer, LDAPSettings settings, LDAPSecuredConfiguration securedConfiguration, List<string> fieldsToLoad = null)
        {
            _logger = logger;
            _settings = settings;
            _securedConfiguration = securedConfiguration;
            _fieldsToLoad = fieldsToLoad;
        }

        public void InitializeConnection()
        {
            _logger.LogInformation("Initializing a connection to LDAP server using legacy provider using authentication: {auth}", _settings.AuthenticationType.ToString());

            _searchRoot = new DirectoryEntry(_settings.Path, _securedConfiguration.UserName, _securedConfiguration.Password, _settings.AuthenticationType);
        }

        public bool IsAuthenticated()
        {
            _logger.LogInformation("Check if LDAP Service is authenticated.");

            try
            {
                object nativeObject = FetchItems(1).ToList();
                return true;
            }
            catch (Exception ex)
            {
                LogAuthenticationError(ex);
                throw;
            }
        }

        public List<string> FetchAllProperties(int? overrideSizeLimit = null)
        {
            IEnumerable<SearchResult> searchResult = FetchItems(overrideSizeLimit);
            return GetAllProperties(searchResult);
        }

        public IEnumerable<SearchResult> FetchItems(int? overrideSizeLimit = null)
        {
            return FetchItems(_settings.Filter, overrideSizeLimit);
        }

        public IEnumerable<SearchResult> FetchItems(string filter, int? overrideSizeLimit)
        {
            return FetchItems(_searchRoot, filter, overrideSizeLimit);
        }

        public IEnumerable<SearchResult> FetchItemsUpTheTree(string filter, int? overrideSizeLimit)
        {
            DirectoryEntry currentSearchRoot = _searchRoot;
            _settings.ImportNested = true;
            List<string> searchedPaths = new List<string>();
            searchedPaths.Add(currentSearchRoot.Path);
            bool isNewPath = true;
            IEnumerable<SearchResult> items = null;
            do
            {
                items = FetchItems(currentSearchRoot, filter, overrideSizeLimit);
                try
                {
                    isNewPath = false;
                    currentSearchRoot = currentSearchRoot.Parent;
                    if (!searchedPaths.Contains(currentSearchRoot.Path))
                    {
                        isNewPath = true;
                        searchedPaths.Add(currentSearchRoot.Path);
                    }
                }
                catch
                {
                    LogFetchItemsUpTheTreeError();
                }
            } while ((items == null || items.Count() == 0) && currentSearchRoot != null && isNewPath);

            return items;
        }

        private IEnumerable<SearchResult> FetchItems(DirectoryEntry searchRoot, string filter, int? overrideSizeLimit)
        {
            using (DirectorySearcher searcher = new DirectorySearcher(searchRoot, filter))
            {
                searcher.AttributeScopeQuery = _settings.AttributeScopeQuery;
                searcher.ExtendedDN = _settings.ExtendedDN;
                searcher.PropertyNamesOnly = _settings.PropertyNamesOnly;
                searcher.ReferralChasing = _settings.ReferralChasing;
                searcher.SearchScope = _settings.SearchScope;
                if (_fieldsToLoad != null && _fieldsToLoad.Count > 0)
                {
                    searcher.PropertiesToLoad.AddRange(_fieldsToLoad.ToArray());
                }

                if (!overrideSizeLimit.HasValue)
                {
                    searcher.PageSize = _settings.PageSize;
                    if (searcher.PageSize == 0)
                    {
                        searcher.SizeLimit = _settings.SizeLimit;
                    }
                }
                else
                {
                    searcher.SizeLimit = overrideSizeLimit.Value;
                }

                IEnumerable<SearchResult> itemList = SafeFindAll(searcher);

                return itemList;
            }
        }

        private IEnumerable<SearchResult> SafeFindAll(DirectorySearcher searcher)
        {
            using (SearchResultCollection results = searcher.FindAll())
            {
                foreach (SearchResult result in results)
                {
                    yield return result;
                }
            } // SearchResultCollection will be disposed here
        }

        private List<string> GetAllProperties(IEnumerable<SearchResult> previewItems)
        {
            HashSet<string> properties = new HashSet<string>();
            foreach (SearchResult item in previewItems)
            {
                foreach (object property in item.Properties.PropertyNames)
                {
                    properties.Add(property.ToString());
                }
            }

            List<string> listProperties = properties.ToList();
            listProperties.Sort();
            return listProperties;
        }

#region Logging

        private void LogFetchItemsUpTheTreeError()
        {
            _logger.LogInformation("Attempting to fetch items in LDAP Service Failed.");
        }

        private void LogAuthenticationError(Exception ex)
        {
            _logger.LogError(ex, "Error occurred during LDAP Service authentication");
        }

        public void Dispose()
        {
            _searchRoot.Dispose();
        }
        #endregion
    }
}
