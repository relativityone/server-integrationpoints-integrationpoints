using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers
{
	public class SavedSearchHelper : HelperBase
	{
		public SavedSearchHelper(HelperManager helperManager, InMemoryDatabase database, ProxyMock proxyMock) : base(helperManager, database, proxyMock)
		{
		}

		public SavedSearchTest CreateSavedSearch(SavedSearchTest savedSearch)
		{
			Database.SavedSearches.Add(savedSearch);

			return savedSearch;
		}
	}
}
