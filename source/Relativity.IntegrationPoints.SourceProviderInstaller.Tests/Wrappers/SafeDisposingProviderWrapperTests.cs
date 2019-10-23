using System;
using System.Collections.Generic;
using System.Data;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.IntegrationPoints.Contracts.Internals;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.SourceProviderInstaller.Internals.Wrappers;

namespace Relativity.IntegrationPoints.SourceProviderInstaller.Tests.Wrappers
{
	[TestFixture]
	public class SafeDisposingProviderWrapperTests
	{
		private Mock<IProviderAggregatedInterfaces> _innerProviderMock;
		private SafeDisposingProviderWrapper _sut;

		[SetUp]
		public void SetUp()
		{
			_innerProviderMock = new Mock<IProviderAggregatedInterfaces>();
			_sut = new SafeDisposingProviderWrapper(_innerProviderMock.Object);
		}

		[Test]
		public void ShouldThrowArgumentExceptionWhenInnerProviderIsNull()
		{
			// arrange
			IProviderAggregatedInterfaces innerProvider = null;

			// act
			Action callConstructorActionWithNullParameter = () => new SafeDisposingProviderWrapper(innerProvider);

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
				.BeOfType<SafeDisposingDataReaderWrapper>(
					"because returned {0} should be wrapped in {1}",
					nameof(IDataReader),
					nameof(SafeDisposingDataReaderWrapper)
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
				.BeOfType<SafeDisposingDataReaderWrapper>(
					"because returned {0} should be wrapped in {1}",
					nameof(IDataReader),
					nameof(SafeDisposingDataReaderWrapper)
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
