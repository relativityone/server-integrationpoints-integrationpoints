using Moq;
using Relativity.Services.Objects.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
	partial class ObjectManagerStub
	{
		public void SetupApplications()
		{
			const int relativityApplicationTypeId = 1000014;
			const string automatekWorkflowsApplicatioName = "Automated Workflows";

			Mock.Setup(x => x.QuerySlimAsync(It.IsAny<int>(), It.Is<QueryRequest>(
					q => q.ObjectType.ArtifactTypeID == relativityApplicationTypeId && q.Condition == $"'Name' == '{automatekWorkflowsApplicatioName}'"),
					It.IsAny<int>(), It.IsAny<int>()))
				.ReturnsAsync(new QueryResultSlim());
		}
	}
}
