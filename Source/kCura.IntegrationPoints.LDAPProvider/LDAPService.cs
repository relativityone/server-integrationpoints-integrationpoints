using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.LDAPProvider
{
	public class LDAPService
	{
		private LDAPSettings _settings;
		private DirectoryEntry _searchRoot;
		private List<string> _fieldsToLoad;

		public LDAPService(LDAPSettings settings, List<string> fieldsToLoad = null)
		{
			_settings = settings;
			_fieldsToLoad = fieldsToLoad;
		}

		public void InitializeConnection()
		{
			_searchRoot = new DirectoryEntry(_settings.Path, _settings.UserName, _settings.Password, _settings.AuthenticationType);
		}

		public bool IsAuthenticated()
		{
			bool authentic = false;
			try
			{
				object nativeObject = FetchItems(1).ToList();
				authentic = true;
			}
			catch (DirectoryServicesCOMException) { }
			catch (COMException) { }
			return authentic;
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
				if (_fieldsToLoad != null && _fieldsToLoad.Count > 0) searcher.PropertiesToLoad.AddRange(_fieldsToLoad.ToArray());

				if (!overrideSizeLimit.HasValue)
				{
					searcher.PageSize = _settings.PageSize;
					if (searcher.PageSize == 0) searcher.SizeLimit = _settings.SizeLimit;
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

		public List<string> GetAllProperties(IEnumerable<SearchResult> previewItems)
		{
			HashSet<string> properties = new HashSet<string>();
			foreach (SearchResult item in previewItems)
			{
				foreach (object property in item.Properties.PropertyNames)
				{
					//GetPropertyType(property.ToString(), item);
					properties.Add(property.ToString());
				}
			}
			//if (properties.Count > 0) properties.Add("path");

			List<string> listProperties = properties.ToList();
			listProperties.Sort();
			return listProperties;
		}

		//private object GetPropertyType(string property, SearchResult item)
		//{
		//	DirectoryEntry de = new DirectoryEntry(item.Path, _settings.UserName, _settings.Password, _settings.AuthenticationType);
		//	DirectoryEntry schema = de.SchemaEntry;

		//	foreach (DirectoryEntry myChildDirectoryEntry in schema.Children)
		//	{
		//		Console.WriteLine(myChildDirectoryEntry.Path);
		//	}

		//	foreach (var prop in schema.Properties.PropertyNames)
		//	{
		//		string propName = prop.ToString();
		//		var propValue = schema.Properties[propName].Value;
		//	}

		//	return null;
		//}
	}
}
