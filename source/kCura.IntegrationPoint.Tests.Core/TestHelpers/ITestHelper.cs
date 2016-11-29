using System;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.API;

namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
	public interface ITestHelper : IHelper
	{
		string RelativityUserName { get; set; }

		string RelativityPassword { get; set; }

		IPermissionRepository PermissionManager { get; }

		T CreateUserProxy<T>() where T : IDisposable;

		T CreateAdminProxy<T>() where T : IDisposable;

		T CreateUserProxy<T>(string username) where T : IDisposable;
	}
}
