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

		public LDAPService(LDAPSettings settings)
		{
			_settings = settings;
		}

		public void InitializeConnection()
		{
			_searchRoot = new DirectoryEntry(_settings.ConnectionPath, _settings.UserName, _settings.Password, _settings.AuthenticationType);
		}

		public bool IsAuthenticated()
		{
			bool authentic = false;
			try
			{
				object nativeObject = _searchRoot.NativeObject;
				authentic = true;
			}
			catch (DirectoryServicesCOMException) { }
			catch (COMException) { }
			return authentic;
		}

		public IEnumerable<SearchResult> FetchUsers()
		{
			using (DirectorySearcher searcher = new DirectorySearcher(_searchRoot, _settings.Filter))
			{
				searcher.AttributeScopeQuery = _settings.AttributeScopeQuery;
				searcher.ExtendedDN = _settings.ExtendedDN;
				searcher.PropertyNamesOnly = _settings.PropertyNamesOnly;
				searcher.ReferralChasing = _settings.ReferralChasing;
				searcher.SearchScope = _settings.SearchScope;

				searcher.PageSize = _settings.PageSize;
				if (searcher.PageSize == 0) searcher.SizeLimit = _settings.SizeLimit;


				IEnumerable<SearchResult> itemList = SafeFindAll(searcher);

				if (itemList.Count() < 1)
				{
					throw new Exception("No matching directory users found.");
				}
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

		//public List<string> GetAllProperties()
		//{
		//	var previewUsers = FetchUsers(group, 100);

		//	HashSet<string> properties = new HashSet<string>();
		//	foreach (SearchResult user in previewUsers)
		//	{
		//		foreach (object p in user.Properties.PropertyNames)
		//		{
		//			properties.Add(p.ToString());
		//		}
		//	}

		//	List<string> listProperties = properties.ToList();
		//	listProperties.Sort();
		//	return listProperties;
		//}
	}
}
