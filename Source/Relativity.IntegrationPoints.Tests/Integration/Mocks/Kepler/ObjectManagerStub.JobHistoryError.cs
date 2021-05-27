using kCura.IntegrationPoints.Data;
using Moq;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Services.Objects.DataContracts;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
	public partial class ObjectManagerStub
	{
		private void SetupJobHistoryError()
		{
			const string conditionPattern = @"\('Job History' IN OBJECT \[(.*)\]\) AND \('Error Type' == CHOICE 9ddc4914-fef3-401f-89b7-2967cd76714b\)";

			Mock.Setup(x => x.QueryAsync(It.IsAny<int>(), It.Is<QueryRequest>(
					q => IsJobHasErrorsQuery(q, conditionPattern)), It.IsAny<int>(), It.IsAny<int>()))
				.ReturnsAsync(new QueryResult
				{
					Objects = new List<RelativityObject>(),
					TotalCount = 0,
					ResultCount = 0
				});
		}

		private bool IsJobHasErrorsQuery(QueryRequest query, string conditionPattern) =>
			query.ObjectType.Guid == ObjectTypeGuids.JobHistoryErrorGuid &&
			Regex.Match(query.Condition, conditionPattern).Success;
	}
}
