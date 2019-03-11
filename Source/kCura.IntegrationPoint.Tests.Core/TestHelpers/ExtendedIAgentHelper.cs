using Relativity.API;
using System;

namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
	public class ExtendedIAgentHelper : IAgentHelper
	{
		private bool _isDisposed = false;

		private readonly ITestHelper _helper;

		public ExtendedIAgentHelper(ITestHelper helper)
		{
			_helper = helper;
		}

		public IDBContext GetDBContext(int caseId)
		{
			return _helper.GetDBContext(caseId);
		}

		public IServicesMgr GetServicesManager()
		{
			return _helper.GetServicesManager();
		}

		public IUrlHelper GetUrlHelper()
		{
			return _helper.GetUrlHelper();
		}

		public ILogFactory GetLoggerFactory()
		{
			return _helper.GetLoggerFactory();
		}

		public string ResourceDBPrepend()
		{
			return _helper.ResourceDBPrepend();
		}

		public string ResourceDBPrepend(IDBContext context)
		{
			return _helper.ResourceDBPrepend(context);
		}

		public string GetSchemalessResourceDataBasePrepend(IDBContext context)
		{
			return _helper.GetSchemalessResourceDataBasePrepend(context);
		}

		public Guid GetGuid(int workspaceId, int artifactId)
		{
			return _helper.GetGuid(workspaceId, artifactId);
		}

		public ISecretStore GetSecretStore()
		{
			return _helper.GetSecretStore();
		}

		public IInstanceSettingsBundle GetInstanceSettingBundle()
		{
			return _helper.GetInstanceSettingBundle();
		}

		public IStringSanitizer GetStringSanitizer(int workspaceID)
		{
			return _helper.GetStringSanitizer(workspaceID);
		}

		public IAuthenticationMgr GetAuthenticationManager()
		{
			throw new NotImplementedException();
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_isDisposed)
			{
				return;
			}

			if (disposing)
			{
				_helper?.Dispose();
			}

			_isDisposed = true;
		}

		public void Dispose()
		{
			Dispose(true);
		}
	}
}
