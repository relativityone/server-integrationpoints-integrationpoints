using System;
using Relativity.API;

namespace Relativity.Sync.Tests.System.Core.Helpers
{
	public class TestHelper : IHelper
	{
		public void Dispose()
		{
		}

		public IDBContext GetDBContext(int caseID)
		{
			return new TestDbContext(caseID);
		}

		#region Not Implemented

		public IServicesMgr GetServicesManager()
		{
			throw new NotImplementedException();
		}

		public IUrlHelper GetUrlHelper()
		{
			throw new NotImplementedException();
		}

		public ILogFactory GetLoggerFactory()
		{
			throw new NotImplementedException();
		}

		public string ResourceDBPrepend()
		{
			throw new NotImplementedException();
		}

		public string ResourceDBPrepend(IDBContext context)
		{
			throw new NotImplementedException();
		}

		public string GetSchemalessResourceDataBasePrepend(IDBContext context)
		{
			throw new NotImplementedException();
		}

		public Guid GetGuid(int workspaceID, int artifactID)
		{
			throw new NotImplementedException();
		}

		public ISecretStore GetSecretStore()
		{
			throw new NotImplementedException();
		}

		public IInstanceSettingsBundle GetInstanceSettingBundle()
		{
			throw new NotImplementedException();
		}

		public IStringSanitizer GetStringSanitizer(int workspaceID)
		{
			throw new NotImplementedException();
		}


		#endregion
	}
}
