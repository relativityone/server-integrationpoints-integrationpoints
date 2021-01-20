﻿using kCura.IntegrationPoints.Common.Context;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Web.Context.UserContext;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.IntegrationPointsServices
{
	internal class ServiceContextHelperForWeb : IServiceContextHelper
	{
		private int? _workspaceId;

		private const int _ADMIN_CASE_WORKSPACE_ARTIFACT_ID = -1;
		private const int _MINIMUM_VALID_WORKSPACE_ARTIFACT_ID = 1;

		private readonly IAPILog _logger;
		private readonly IUserContext _userContext;
		private readonly IHelper _helper;
		private readonly IWorkspaceContext _workspaceContext;

		public ServiceContextHelperForWeb(
			IAPILog logger,
			IHelper helper,
			IWorkspaceContext workspaceContext,
			IUserContext userContext)
		{
			_logger = logger.ForContext<ServiceContextHelperForWeb>();
			_helper = helper;
			_workspaceContext = workspaceContext;
			_userContext = userContext;
		}


		public int WorkspaceID
		{
			get
			{
				if (!_workspaceId.HasValue)
				{
					_workspaceId = _workspaceContext.GetWorkspaceID();
				}

				return _workspaceId.Value;
			}
		}

		public int GetEddsUserID()
		{
			return _userContext.GetUserID();
		}

		public int GetWorkspaceUserID()
		{
			return _userContext.GetWorkspaceUserID();
		}

		public IRelativityObjectManagerService GetRelativityObjectManagerService()
		{
			if (WorkspaceID < _MINIMUM_VALID_WORKSPACE_ARTIFACT_ID)
			{
				_logger.LogWarning(
					"Cannot create {service} because workspaceId is invalid: {workspaceId}", 
					nameof(IRelativityObjectManagerService), 
					WorkspaceID
				);
				return null;
			}

			return ServiceContextFactory.CreateRelativityObjectManagerService(_helper, WorkspaceID);
		}

		public IDBContext GetDBContext(int workspaceId = _ADMIN_CASE_WORKSPACE_ARTIFACT_ID)
		{
			return _helper.GetDBContext(workspaceId);
		}
	}
}
