using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.DocumentTransferProvider.Tests.Helpers
{
	public class ArgumentMatcher
	{
		public static bool DocumentSearchProviderQueriesMatch(Query<Document> expected, Query<Document> actual)
		{
			if (expected == null && actual == null)
			{
				return true;
			}
			if (expected == null && actual != null)
			{
				return false;
			}
			if (expected != null && actual == null)
			{
				return false;
			}

			if (((SavedSearchCondition) expected.Condition).ArtifactID != ((SavedSearchCondition) actual.Condition).ArtifactID)
			{
				return false;
			}

			if (expected.Fields.Count != actual.Fields.Count)
			{
				return false;
			}

			for (int i = 0; i < expected.Fields.Count; i++)
			{
				if (expected.Fields[i].Name != actual.Fields[i].Name)
				{
					return false;
				}

			}

			return true;
		}
	}
}