using System;
using System.Data;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core.Exceptions;
using kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes;
using kCura.IntegrationPoints.Core.Services.Domain;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Tests.Integration.DataProviderBuilderTests.Mocks;
using NUnit.Framework;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.Testing.Identification;

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
	[Feature.DataTransfer.IntegrationPoints]
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

		[IdentifiedTest("58e1a8ae-2f28-40dc-985c-793576a660f1")]
		[SmokeTest]
		public void ShouldReturnDataProviderWhenExists()
		{
			// act
			IDataSourceProvider dataProvider = _sut.GetDataProvider(Guid.Empty, _providerGuid);

			// assert
			dataProvider.Should()
				.NotBeNull("because provider with this Guid exists");
		}

		[IdentifiedTest("35d86ea1-a2c6-483c-b464-6a34e2a8fb39")]
		[SmokeTest]
		public void ShouldThrowExceptionWhenProviderNotExists()
		{
			// arrange
			Guid nonExistingProviderGuid = Guid.NewGuid();

			// act
			Action getProviderAction = () => _sut.GetDataProvider(Guid.Empty, nonExistingProviderGuid);

			// assert
			getProviderAction
				.Should().Throw<IntegrationPointsException>("because provider with this Guid does not exists");
		}

		[IdentifiedTest("984d08b9-3215-40be-8bd8-7b0c2af25daa")]
		[SmokeTest]
		public void ShouldReturnDisposableProvider()
		{
			// act
			IDataSourceProvider dataProvider = _sut.GetDataProvider(Guid.Empty, _providerGuid);

			// assert
			dataProvider.Should().BeAssignableTo<IDisposable>("becasue it should return disposable wrapper");
		}

		[IdentifiedTest("54d57a0a-222c-44d6-8e59-abb10e7a9941")]
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
				.Should().NotThrow("because provider should be wrapped in safe disposing wrapper");
		}

		[IdentifiedTest("af127aa7-4c0e-475c-be47-477de69505b5")]
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
				.Should().NotThrow("because DataReader returned by GetData should be wrapped in safe disposing wrapper");
		}

		[IdentifiedTest("30ba2466-1160-456a-8304-77efd96aea46")]
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
				.Should().NotThrow("because DataReader returned by GetData should be wrapped in safe disposing wrapper");
		}
	}
}
