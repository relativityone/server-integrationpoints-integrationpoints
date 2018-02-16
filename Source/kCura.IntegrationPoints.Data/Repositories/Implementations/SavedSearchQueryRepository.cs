using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class SavedSearchQueryRepository : KeplerServiceBase, ISavedSearchQueryRepository
	{
		private const string _NAME_FIELD = "Name";
		private const string _OWNER_FIELD = "Owner";

		public SavedSearchQueryRepository(IRelativityObjectManager relativityObjectManager) : base(relativityObjectManager)
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
			var query = new QueryRequest
			{
				ObjectType = new ObjectTypeRef() { ArtifactTypeID = (int)ArtifactType.Search },
				Condition = condition,
				Fields = new[] { new FieldRef { Name = "Name" }, new FieldRef { Name = "Owner" } }
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
