using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Domain.Wrappers;
using Moq;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Domain.Tests.Wrappers
{
	[TestFixture]
	public class ProviderCrossAppDomainWrapperTests
	{
		private Mock<IDataSourceProvider> _innerProviderMock;
		private ProviderCrossAppDomainWrapper _sut;

		[SetUp]
		public void SetUp()
		{
			_innerProviderMock = new Mock<IDataSourceProvider>();
			_sut = new ProviderCrossAppDomainWrapper(_innerProviderMock.Object);
		}

		[Test]
		public void ShouldThrowArgumentExceptionWhenInnerProviderIsNull()
		{
			// arrange
			IProviderAggregatedInterfaces innerProvider = null;

			// act
			Action callConstructorActionWithNullParameter = () => new ProviderSafeDisposeWrapper(innerProvider);

			// assert
			callConstructorActionWithNullParameter.ShouldThrow<ArgumentNullException>();
		}

		[Test]
		public void InitializeLifetimeService_ShouldReturnNull()
		{
			// act
			object result = _sut.InitializeLifetimeService();

			// assert
			result.Should().BeNull("because remote object should not be garbage collected");
		}

		[Test]
		public void Dispose_ShouldDisconnectRemoteObject()
		{
			// arrange
			ObjRef realObjectReference = RemotingServices.Marshal(_sut);

			// act
			_sut.Dispose();

			// assert
			realObjectReference.URI.Should().BeNull("because remote object should be disconnected");
		}

		[Test]
		public void GetData_ShouldWrapDataReaderInDataReaderCrossAppDomainWrapper()
		{
			// arrage
			var innerDataReaderMock = new Mock<IDataReader>();
			SetupInnerDataReaderGetData(innerDataReaderMock.Object);

			// act
			IDataReader returnedDataReader = _sut.GetData(
				fields: null,
				entryIds: null,
				providerConfiguration: null
			);

			// assert
			returnedDataReader.Should()
				.BeOfType<DataReaderCrossAppDomainWrapper>(
					"because returned {0} should be wrapped in {1}",
					nameof(IDataReader),
					nameof(DataReaderCrossAppDomainWrapper)
				);
		}

		[Test]
		public void GetData_ShouldReturnNullIfDecoratedMethodReturnsNull()
		{
			// arrage
			IDataReader innerDataReader = null;
			SetupInnerDataReaderGetData(innerDataReader);

			// act
			IDataReader returnedDataReader = _sut.GetData(
				fields: null,
				entryIds: null,
				providerConfiguration: null
			);

			// assert
			returnedDataReader.Should()
				.BeNull(
					"because returned {0} was null",
					nameof(IDataReader)
				);
		}

		[Test]
		public void GetBatchableIds_ShouldWrapDataReaderInDataReaderCrossAppDomainWrapper()
		{
			// arrage
			var innerDataReaderMock = new Mock<IDataReader>();
			SetupInnerDataReaderGetBatchableIds(innerDataReaderMock.Object);

			// act
			IDataReader returnedDataReader = _sut.GetBatchableIds(
				identifier: null,
				providerConfiguration: null
			);

			// assert
			returnedDataReader.Should()
				.BeOfType<DataReaderCrossAppDomainWrapper>(
					"because returned {0} should be wrapped in {1}",
					nameof(IDataReader),
					nameof(DataReaderCrossAppDomainWrapper)
				);
		}

		[Test]
		public void GetBatchableIds_ShouldReturnNullIfDecoratedMethodReturnsNull()
		{
			// arrage
			IDataReader innerDataReader = null;
			SetupInnerDataReaderGetBatchableIds(innerDataReader);

			// act
			IDataReader returnedDataReader = _sut.GetBatchableIds(
				identifier: null,
				providerConfiguration: null
			);

			// assert
			returnedDataReader.Should()
				.BeNull(
					"because returned {0} was null",
					nameof(IDataReader)
				);
		}

		private void SetupInnerDataReaderGetData(IDataReader innerDataReader)
		{
			_innerProviderMock
				.Setup(x =>
					x.GetData(
						It.IsAny<IEnumerable<FieldEntry>>(),
						It.IsAny<IEnumerable<string>>(),
						It.IsAny<DataSourceProviderConfiguration>()
					)
				)
				.Returns(innerDataReader);
		}

		private void SetupInnerDataReaderGetBatchableIds(IDataReader innerDataReader)
		{
			_innerProviderMock
				.Setup(x =>
					x.GetBatchableIds(
						It.IsAny<FieldEntry>(),
						It.IsAny<DataSourceProviderConfiguration>()
					)
				)
				.Returns(innerDataReader);
		}
	}
}
