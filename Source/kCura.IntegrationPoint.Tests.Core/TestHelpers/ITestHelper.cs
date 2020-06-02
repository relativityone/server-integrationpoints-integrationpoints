using System;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.API;
using Relativity.Telemetry.Services.Metrics;

namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
	using WinEDDS.Service.Export;

	public interface ITestHelper : ICPHelper
	{
		string RelativityUserName { get; }

		string RelativityPassword { get; }

		T CreateProxy<T>() where T : IDisposable;

		T CreateProxy<T>(string username) where T : IDisposable;

		void InjectProxy<T>(T proxy) where T : IDisposable;

		ISearchManager CreateSearchManager();

		ITestHelper CreateHelperForUser(string userName, string password);
	}
}
