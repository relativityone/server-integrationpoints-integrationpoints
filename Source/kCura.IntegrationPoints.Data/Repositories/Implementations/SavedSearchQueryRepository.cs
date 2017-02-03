using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.RDO;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class SavedSearchQueryRepository : KeplerServiceBase, ISavedSearchQueryRepository
	{
		private const string _NAME_FIELD = "Name";
		private const string _OWNER_FIELD = "Owner";

		public SavedSearchQueryRepository(IObjectQueryManagerAdaptor objectQueryManagerAdaptor) : base(objectQueryManagerAdaptor)
		{
		}

		public SavedSearchDTO RetrieveSavedSearch(int savedSearchId)
		{
			return GetSavedSearchesDto($"'Artifact ID' == {savedSearchId}").FirstOrDefault();
		}

		public IEnumerable<SavedSearchDTO> RetrievePublicSavedSearches()
		{
			return GetSavedSearchesDto().Where(item => string.IsNullOrEmpty(item.Owner));
		}

		private IEnumerable<SavedSearchDTO> GetSavedSearchesDto(string condition = null)
		{
			var query = new global::Relativity.Services.ObjectQuery.Query()
			{
				Condition = condition,
				Fields = new[] { "Name", "Owner" },
				TruncateTextFields = false,
			};

			ArtifactDTO[] results = this.RetrieveAllArtifactsAsync(query).GetResultsWithoutContextSync();

			var savedSearches = results.Select(item => new SavedSearchDTO
			{
				ArtifactId = item.ArtifactId,
				Name = item.Fields.FirstOrDefault(field => field.Name == _NAME_FIELD)?.Value as string,
				Owner = item.Fields.FirstOrDefault(field => field.Name == _OWNER_FIELD)?.Value as string
			});

			return savedSearches;
		}
	}
}
