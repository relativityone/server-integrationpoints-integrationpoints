using System;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using Relativity.API;

namespace kCura.IntegrationPoints.Core
{
	public class ServiceContextHelperForLoadFileReader : IServiceContextHelper
	{
		private readonly int _workspaceId;
		public ServiceContextHelperForLoadFileReader(int workspaceId)
		{
			_workspaceId = workspaceId;
		}
		public int WorkspaceID
		{
			get
			{
				return _workspaceId;
			}

			set
			{
				throw new NotImplementedException();
			}
		}

		public IDBContext GetDBContext(int workspaceID = -1)
		{
			throw new NotImplementedException();
		}

		public int GetEddsUserID()
		{
			throw new NotImplementedException();
		}

		public IRSAPIService GetRsapiService()
		{
			throw new NotImplementedException();
		}

		public int GetWorkspaceUserID()
		{
			throw new NotImplementedException();
		}
	}
}
