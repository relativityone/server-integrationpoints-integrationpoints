using Moq;
using Relativity.Services.Objects.DataContracts;
using System.Collections.Generic;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
	public partial class ObjectManagerStub
	{
		private void SetupObjectType()
		{
			Mock.Setup(x => x.QuerySlimAsync(It.IsAny<int>(), It.Is<QueryRequest>(
					q => IsObjectTypeQuery(q)), It.IsAny<int>(), It.IsAny<int>()))
				.ReturnsAsync(new QueryResultSlim 
				{
					Objects = new List<RelativityObjectSlim>() { new RelativityObjectSlim() },
					TotalCount = 1
				});
		}

		private bool IsObjectTypeQuery(QueryRequest query)
		{
			return query.ObjectType.ArtifactTypeID == (int)ArtifactType.ObjectType;
		}
	}
}
