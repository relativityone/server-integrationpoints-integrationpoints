using System.Data;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Core.Tests.Services.Export
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

		public static bool DataTablesMatch(DataTable expected, DataTable actual)
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

			if (expected.Columns.Count != actual.Columns.Count)
			{
				return false;
			}

			if (expected.Columns.Count == 0 
				&& actual.Columns.Count == 0)
			{
				return true;
			}

			for (int i = 0; i < expected.Columns.Count; i++)
			{
				if (!expected.Columns[i].ColumnName.Equals(actual.Columns[i].ColumnName))
				{
					return false;
				}
			}

			return true;
		}
	}
}