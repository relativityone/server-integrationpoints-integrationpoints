using System;
using System.Data;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core.Exceptions;
using kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Core.Services.Domain;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Tests.Integration.DataProviderBuilderTests.Mocks;
using NUnit.Framework;

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

		[Test]
		[SmokeTest]
		public void ShouldReturnDataProviderWhenExists()
		{
			// act
			IDataSourceProvider dataProvider = _sut.GetDataProvider(Guid.Empty, _providerGuid);

			// assert
			dataProvider.Should()
				.NotBeNull("because provider with this Guid exists");
		}

		[Test]
		[SmokeTest]
		public void ShouldThrowExceptionWhenProviderNotExists()
		{
			// arrange
			Guid nonExistingProviderGuid = Guid.NewGuid();

			// act
			Action getProviderAction = () => _sut.GetDataProvider(Guid.Empty, nonExistingProviderGuid);

			// assert
			getProviderAction
				.ShouldThrow<IntegrationPointsException>("because provider with this Guid does not exists");
		}

		[Test]
		[SmokeTest]
		public void ShouldReturnDisposableProvider()
		{
			// act
			IDataSourceProvider dataProvider = _sut.GetDataProvider(Guid.Empty, _providerGuid);

			// assert
			dataProvider.Should().BeAssignableTo<IDisposable>("becasue it should return disposable wrapper");
		}

		[Test]
		[SmokeTest]
		public void ShouldReturnSafeDisposingProvider()
		{
			// arrange
			IDataSourceProvider dataProvider = _sut.GetDataProvider(Guid.Empty, _providerGuid);

			if (!(dataProvider is IDisposable disposableDataProvider))
			{
				throw new TestSetupException("Returned provider should be disposable");
			}
			disposableDataProvider.Dispose();

			Action secondCallToDisposeAction = () => disposableDataProvider.Dispose();

			// act & assert
			secondCallToDisposeAction
				.ShouldNotThrow("because provider should be wrapped in safe disposing wrapper");
		}

		[Test]
		[SmokeTest]
		public void DataReaderReturnedByProvidersGetDataShouldBeSafeDisposing()
		{
			// arrange
			IDataSourceProvider dataProvider = _sut.GetDataProvider(Guid.Empty, _providerGuid);
			IDataReader dataReader = dataProvider.GetData(
				fields: null,
				entryIds: null,
				providerConfiguration: null
			);

			dataReader.Dispose();
			Action secondCallToDisposeAction = () => dataReader.Dispose();

			// act & assert
			secondCallToDisposeAction
				.ShouldNotThrow("because DataReader returned by GetData should be wrapped in safe disposing wrapper");
		}

		[Test]
		[SmokeTest]
		public void DataReaderReturnedByProvidersGetBatchableIdsShouldBeSafeDisposing()
		{
			// arrange
			IDataSourceProvider dataProvider = _sut.GetDataProvider(Guid.Empty, _providerGuid);
			IDataReader dataReader = dataProvider.GetBatchableIds(
				identifier: null,
				providerConfiguration: null
			);

			dataReader.Dispose();
			Action secondCallToDisposeAction = () => dataReader.Dispose();

			// act & assert
			secondCallToDisposeAction
				.ShouldNotThrow("because DataReader returned by GetData should be wrapped in safe disposing wrapper");
		}
	}
}
