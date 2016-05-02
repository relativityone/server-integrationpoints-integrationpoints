﻿namespace kCura.IntegrationPoints.Data.Tests.Integration.Queries
{
	using System;
	using kCura.IntegrationPoints.Data.Queries;
	using kCura.Relativity.Client;
	using NSubstitute;
	using NUnit.Framework;

	[TestFixture]
	public class GetSavedSearchesQueryTest
	{
		[Test]
		public void GetSavedSearchQuery_QueryOnSearchArtifact()
		{
			IRSAPIClient client = Substitute.For<IRSAPIClient>();
			client.APIOptions = new APIOptions();

			GetSavedSearchesQuery getSavedSearchesQuery = new GetSavedSearchesQuery(client);
			getSavedSearchesQuery.ExecuteQuery();

			client.Received().Query(Arg.Is<APIOptions>(option => option == client.APIOptions),
				Arg.Is<Query>(query =>
							query.ArtifactTypeID == (Int32)ArtifactType.Search
							&& query.Condition == null
							&& query.Sorts.Count == 1
							&& query.Sorts[0].Field == "Name"
							&& query.Sorts[0].Direction == SortEnum.Ascending));
		}
	}
}