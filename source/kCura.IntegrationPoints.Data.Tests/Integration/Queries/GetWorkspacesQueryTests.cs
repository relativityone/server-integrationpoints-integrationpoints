namespace kCura.IntegrationPoints.Data.Tests.Integration.Queries
{
	using System;
	using Data.Queries;
	using kCura.Relativity.Client;
	using NSubstitute;
	using NUnit.Framework;

	[TestFixture]
	public class GetWorkspacesQueryTests
	{
		[Test]
		public void GetWorkspacesQuery_QueryOnWorkspaceArtifact()
		{
			IRSAPIClient client = Substitute.For<IRSAPIClient>();
			client.APIOptions = new APIOptions();

			GetWorkspacesQuery getWorkspacesQuery = new GetWorkspacesQuery(client);
			getWorkspacesQuery.ExecuteQuery();

			client.Received().Query(Arg.Is<APIOptions>(option => option == client.APIOptions),
				Arg.Is<Query>(query =>
							query.ArtifactTypeID == (Int32)ArtifactType.Case &&
							query.Condition == null));
		}
	}
}