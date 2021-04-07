using System;
using System.Data;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core.Exceptions;
using kCura.IntegrationPoints.Core.Services.Domain;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Tests.Integration.DataProviderBuilderTests.Mocks;
using NUnit.Framework;
using Relativity.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.Domain.Tests.Integration.DataProviderBuilderTests
{
	/// <summary>
	/// This fixture tests <see cref="DataProviderBuilder"/> and its internal components.
	/// Mocks are used for the following types:
	/// <see cref="ProviderFactoryLifecycleStrategyMock"/> - mock creates new AppDomain and returns
	/// mock factory which lives in this newly created AppDomain;
	/// <see cref="ProviderFactoryMock"/> - it always returns new provider mock;
	/// <see cref="IDataSourceProvider"/> and <see cref="IDataReader"/>;
	/// </summary>
	[TestFixture]
	public class DataProviderBuilderTests
	{
		private DataProviderBuilder _sut;

		private static readonly Guid _providerGuid = Guid.Parse(PROVIDER_GUID_AS_STRING);
		public const string PROVIDER_GUID_AS_STRING = "1af1f2c2-9670-4d6e-a3e9-dbc83db6c232";

		[SetUp]
		public void SetUp()
		{
			string testAssemblyDirectory = TestContext.CurrentContext.TestDirectory;
			var providerFactoryLifecycleStrategyMock = new ProviderFactoryLifecycleStrategyMock(testAssemblyDirectory);

			var providerFactoryVendor = new ProviderFactoryVendor(providerFactoryLifecycleStrategyMock);
			_sut = new DataProviderBuilder(providerFactoryVendor);
		}
	}
}
