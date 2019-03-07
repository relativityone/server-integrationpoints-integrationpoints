﻿using kCura.IntegrationPoints.Data;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.ServiceContext
{
	public class ServiceContextHelperForAgent : IServiceContextHelper
	{
		private readonly IAgentHelper _helper;

		public ServiceContextHelperForAgent(IAgentHelper helper, int workspaceId)
		{
			_helper = helper;
			WorkspaceID = workspaceId;
		}
		
		public int WorkspaceID { get; }

		public int GetEddsUserID() => _helper.GetAuthenticationManager().UserInfo.ArtifactID;

		public int GetWorkspaceUserID() => _helper.GetAuthenticationManager().UserInfo.WorkspaceUserArtifactID;

		public IDBContext GetDBContext() => _helper.GetDBContext(this.WorkspaceID);

		public IRSAPIService GetRsapiService()
		{
			return WorkspaceID > 0
				? ServiceContextFactory.CreateRSAPIService(_helper, WorkspaceID)
				: null;
		}

		public IDBContext GetDBContext(int workspaceID = -1) => _helper.GetDBContext(workspaceID);
	}
}
