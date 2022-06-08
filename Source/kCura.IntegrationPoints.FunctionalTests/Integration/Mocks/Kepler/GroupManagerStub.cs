using System.Collections.Generic;
using Moq;
using Relativity.Services.Interfaces.Group;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
	public class GroupManagerStub : KeplerStubBase<IGroupManager>
	{
		private readonly FakeUser _user;

		public GroupManagerStub(FakeUser user)
		{
			_user = user;
		}

		public void SetupGroupManager()
		{
			Mock.Setup(x => x.QueryGroupsByUserAsync(It.Is<QueryRequest>(q =>
					q.Condition == "'Group Type' == 'System Admin'"), 0, 1, It.IsAny<int>()))
				.ReturnsAsync(_user.IsAdmin
					? new QueryResultSlim {Objects = new List<RelativityObjectSlim> {new RelativityObjectSlim()}}
					: new QueryResultSlim {Objects = new List<RelativityObjectSlim>()}
				);
		}
	}
}
