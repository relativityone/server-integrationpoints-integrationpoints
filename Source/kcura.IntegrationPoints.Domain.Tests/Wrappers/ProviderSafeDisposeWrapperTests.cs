using FluentAssertions;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Domain.Wrappers;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;

namespace kCura.IntegrationPoints.Domain.Tests.Wrappers
{
	[TestFixture]
	public class ProviderSafeDisposeWrapperTests
	{
		private Mock<IProviderAggregatedInterfaces> _innerProviderMock;
		private ProviderSafeDisposeWrapper _sut;

		[SetUp]
		public void SetUp()
		{
			_innerProviderMock = new Mock<IProviderAggregatedInterfaces>();
			_sut = new ProviderSafeDisposeWrapper(_innerProviderMock.Object);
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
		public void GetData_ShouldWrapDataReaderInDataReaderSafeDisposeWrapper()
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
				.BeOfType<DataReaderSafeDisposeWrapper>(
					"because returned {0} should be wrapped in {1}",
					nameof(IDataReader),
					nameof(DataReaderSafeDisposeWrapper)
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
		public void GetBatchableIds_ShouldWrapDataReaderInDataReaderSafeDisposeWrapper()
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
				.BeOfType<DataReaderSafeDisposeWrapper>(
					"because returned {0} should be wrapped in {1}",
					nameof(IDataReader),
					nameof(DataReaderSafeDisposeWrapper)
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

		[Test]
		public void Dispose_ShouldDisposeInnerProvider()
		{
			// act
			_sut.Dispose();

			// assert
			_innerProviderMock.Verify(x => x.Dispose(), Times.Once);
		}

		[Test]
		public void Dispose_ShouldDisposeInnerProviderExactlyOnceEvenWhenDisposeCalledMultipleTimes()
		{
			// act
			_sut.Dispose();
			_sut.Dispose();
			_sut.Dispose();

			// assert
			_innerProviderMock.Verify(x => x.Dispose(), Times.Once);
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
