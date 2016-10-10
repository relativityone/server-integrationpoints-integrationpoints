using System.Collections.Generic;
using kCura.IntegrationPoints.Data.Queries;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Data.Tests.Integration.Queries
{
	[TestFixture]
	public class GetWorkspacesQueryTests
	{

		[Test]
		public void GetWorkspacesQuery_QueryOnWorkspaceArtifact()
		{
			// Arrange
			IRSAPIClient client = Substitute.For<IRSAPIClient>();
			GetWorkspacesQueryTester getWorkspacesQuery = new GetWorkspacesQueryTester(client);
			var expectedWorkspace = new Query<Workspace>()
			{
				Fields = new List<FieldValue>() { new FieldValue() { Name = "Name" } },
				Sorts = new List<Sort>()
				{
					new Sort()
					{
						Field = "Name",
						Direction = SortEnum.Ascending
					}
				}
			};
	

			// Act
			getWorkspacesQuery.ExecuteQuery();

			// Assert
			Assert.IsTrue(this.WorkspaceQueriesMatch(expectedWorkspace, getWorkspacesQuery.LastunQuery), "The RSAPI query should be as expected");
		}

		private class GetWorkspacesQueryTester : GetWorkspacesQuery
		{
			public Query<Workspace> LastunQuery { get; private set; }

			public GetWorkspacesQueryTester(IRSAPIClient client) : base(client)
			{
			}

			public override QueryResultSet<Workspace> ExecuteWorkspaceQuery(Query<Workspace> workspaceQuery)
			{
				LastunQuery = workspaceQuery;

				return null;
			}
		}

		private bool WorkspaceQueriesMatch(Query<Workspace> expected, Query<Workspace> actual)
		{
			if (expected == null && actual != null)
			{
				return false;
			}

			if (expected != null && actual == null)
			{
				return false;
			}

			if (expected.Condition != actual.Condition)
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

			for (int i = 0; i < expected.Sorts.Count; i++)
			{
				if (expected.Sorts[i].Field != actual.Sorts[i].Field
					|| expected.Sorts[i].Direction != actual.Sorts[i].Direction)
				{
					return false;
				}
			}

			return true;
		}
	}
}