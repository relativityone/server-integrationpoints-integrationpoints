using System.Data;
using System.Runtime.Remoting;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.IntegrationPoints.SourceProviderInstaller.Internals.Wrappers;

namespace Relativity.IntegrationPoints.SourceProviderInstaller.Tests.Wrappers
{
	[TestFixture]
	public class CrossAppDomainDataReaderWrapperTests : SafeDisposingDataReaderWrapperTests
	{
		private Mock<IDataReader> _innerDataReaderMock;
		private CrossAppDomainDataReaderWrapper _sut;

		[SetUp]
#pragma warning disable RG1004 // New Inheritance Qualifier Rule
		public new void SetUp()
#pragma warning restore RG1004 // New Inheritance Qualifier Rule
		{
			base.SetUp();

			_innerDataReaderMock = new Mock<IDataReader>();
			_sut = new CrossAppDomainDataReaderWrapper(_innerDataReaderMock.Object);
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
	}
}
