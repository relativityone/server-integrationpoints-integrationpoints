using System;
using System.Data;
using FluentAssertions;
using kCura.IntegrationPoints.Domain.Wrappers;
using Moq;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Domain.Tests.Wrappers
{
	[TestFixture]
	public class DataReaderSafeDisposeWrapperTests
	{
		private Mock<IDataReader> _innerDataReaderMock;
		private DataReaderSafeDisposeWrapper _sut;

		[SetUp]
		public void SetUp()
		{
			_innerDataReaderMock = new Mock<IDataReader>();
			_sut = new DataReaderSafeDisposeWrapper(_innerDataReaderMock.Object);
		}

		[Test]
		public void ShouldThrowArgumentExceptionWhenInnerDataReaderIsNull()
		{
			// arrange
			IDataReader innerDataReader = null;

			// act
			Action callConstructorActionWithNullParameter = () => new DataReaderSafeDisposeWrapper(innerDataReader);

			// assert
			callConstructorActionWithNullParameter.ShouldThrow<ArgumentNullException>();
		}

		[Test]
		public void Dispose_ShouldDisposeInnerDataReader()
		{
			// act
			_sut.Dispose();

			// assert
			_innerDataReaderMock.Verify(x => x.Dispose(), Times.Once);
		}

		[Test]
		public void Dispose_ShouldDisposeInnerDataReaderExactlyOnceEvenWhenDisposeCalledMultipleTimes()
		{
			// act
			_sut.Dispose();
			_sut.Dispose();
			_sut.Dispose();

			// assert
			_innerDataReaderMock.Verify(x => x.Dispose(), Times.Once);
		}
	}
}
