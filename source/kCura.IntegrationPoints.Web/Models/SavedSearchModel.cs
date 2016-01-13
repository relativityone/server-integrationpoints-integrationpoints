namespace kCura.IntegrationPoints.Web.Models
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using kCura.IntegrationPoints.Data.Queries;
	using kCura.Relativity.Client;

	public class SavedSearchModel
	{
		private SavedSearchModel()
		{
		}

		public int Value { get; private set; }
		public String DisplayName { get; private set; }

		public static IList<SavedSearchModel> GetAllSavedSearches(IRSAPIClient context)
		{
			const String identifier = "Text Identifier";

			GetSavedSearchesQuery query = new GetSavedSearchesQuery(context);
			List<Artifact> artifacts = query.ExecuteQuery().QueryArtifacts;
			List<SavedSearchModel> result = new List<SavedSearchModel>(artifacts.Count);
			foreach (var artifact in artifacts)
			{
				Field textIdentifierField = artifact.getFieldByName(identifier);
				if (textIdentifierField != null && textIdentifierField.Value != null)
				{
					String searchName = Encoding.Default.GetString((byte[])artifact.getFieldByName(identifier).Value);
					result.Add(new SavedSearchModel() { DisplayName = searchName, Value = artifact.ArtifactID });
				}
			}
			return result;
		}
	}
}