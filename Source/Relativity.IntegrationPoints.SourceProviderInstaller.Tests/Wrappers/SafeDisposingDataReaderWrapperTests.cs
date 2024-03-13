using System;
using System.Data;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.IntegrationPoints.SourceProviderInstaller.Internals.Wrappers;

namespace Relativity.IntegrationPoints.SourceProviderInstaller.Tests.Wrappers
{
	[TestFixture]
	public class SafeDisposingDataReaderWrapperTests
	{
		private Mock<IDataReader> _innerDataReaderMock;
		private SafeDisposingDataReaderWrapper _sut;

		[SetUp]
		public void SetUp()
		{
			_innerDataReaderMock = new Mock<IDataReader>();
			_sut = new SafeDisposingDataReaderWrapper(_innerDataReaderMock.Object);
		}

		[Test]
		public void ShouldThrowArgumentExceptionWhenInnerDataReaderIsNull()
		{
			// arrange
			IDataReader innerDataReader = null;

			// act
			Action callConstructorActionWithNullParameter = () => new SafeDisposingDataReaderWrapper(innerDataReader);

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
