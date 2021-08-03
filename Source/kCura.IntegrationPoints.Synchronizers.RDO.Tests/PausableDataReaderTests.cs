using System.Data;
using FluentAssertions;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport.Implementations;
using Moq;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Synchronizers.RDO.Tests
{
    [TestFixture]
    public class PausableDataReaderTests
    {
        private PausableDataReader _sut;
        private Mock<IJobStopManager> _jobStopManagerMock;
        private Mock<IDataReader> _dataReaderMock;

        [SetUp]
        public void Setup()
        {
            _jobStopManagerMock = new Mock<IJobStopManager>();
            _jobStopManagerMock.Setup(x => x.ShouldDrainStop).Returns(true);

            _dataReaderMock = new Mock<IDataReader>();

            _sut = new PausableDataReader(_dataReaderMock.Object, _jobStopManagerMock.Object);
        }
        
        [TestCase(true)]
        [TestCase(false)]
        public void Read_ShouldPassInnerReaderReadResult(bool innerReadResult)
        {
            // Arrange
            _dataReaderMock.Setup(x => x.Read()).Returns(innerReadResult);
            
            // Act
            bool result = _sut.Read();
            
            // Assert
            result.Should().Be(innerReadResult);
        }

        [Test]
        public void Read_ShouldReturnFalse_WhenDrainStopRequested()
        {
            // Arrange
            _dataReaderMock.Setup(x => x.Read()).Returns(true);
            
            _sut.Read(); // one read for IAPI
            _jobStopManagerMock.Setup(x => x.ShouldDrainStop).Returns(true);
            
            // Act && Assert
            _sut.Read().Should().BeFalse();
        }
        
        [Test]
        public void Read_ShouldReadInnerReaderAtLeastOnce_WhenDrainStopIsRequested()
        {
            // Act
            _sut.Read();
            
            // Assert
            _dataReaderMock.Verify(x => x.Read(), Times.Once);
        }
    }
}