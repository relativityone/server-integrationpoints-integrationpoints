using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks
{
    public class FakeCaseServiceContext: IServiceContextHelper
	{
		private readonly IHelper _helper;
		private int UserArtifactId;

		public FakeCaseServiceContext(IHelper helper, int workspaceArtifactId, int userArtifactId)
		{
			_helper = helper;
			WorkspaceID = workspaceArtifactId;
			UserArtifactId = userArtifactId;
		}

		public int WorkspaceID { get; }

		public int GetEddsUserID()
		{
			return UserArtifactId;
		}

		public int GetWorkspaceUserID()
		{
			return UserArtifactId;
		}

		public IDBContext GetDBContext(int workspaceID = -1)
		{
			return _helper.GetDBContext(workspaceID);
		}

		public IRelativityObjectManagerService GetRelativityObjectManagerService()
		{
			return ServiceContextFactory.CreateRelativityObjectManagerService(_helper, WorkspaceID);
		}
	}
}
