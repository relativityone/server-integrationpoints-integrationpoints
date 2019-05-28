﻿using System;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.API;

namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
	using WinEDDS.Service.Export;

	public interface ITestHelper : ICPHelper
	{
		string RelativityUserName { get; }

		string RelativityPassword { get; }

		T CreateUserProxy<T>() where T : IDisposable;

		T CreateAdminProxy<T>() where T : IDisposable;

		T CreateUserProxy<T>(string username) where T : IDisposable;

		ISearchManager CreateSearchManager();

		ITestHelper CreateHelperForUser(string userName, string password);
	}
}
