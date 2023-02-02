using System;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Error;
using Relativity.Services.Exceptions;

namespace kCura.IntegrationPoints.Data.Tests.Queries
{
    [TestFixture, Category("Unit")]
    public class CreateErrorRdoQueryTests
    {
        private Mock<IAPILog> _logger;
        private Mock<IServicesMgr> _servicesMgr;
        private Mock<IErrorManager> _errorManager;

        private Data.Queries.CreateErrorRdoQuery _sut;

        [SetUp]
        public void SetUp()
        {
            _errorManager = new Mock<IErrorManager>();
            _servicesMgr = new Mock<IServicesMgr>();
            _servicesMgr.Setup(x => x.CreateProxy<IErrorManager>(It.IsAny<ExecutionIdentity>())).Returns(_errorManager.Object);
            _logger = new Mock<IAPILog>();
            _logger.Setup(x => x.ForContext<Data.Queries.CreateErrorRdoQuery>()).Returns(_logger.Object);

            _sut = new Data.Queries.CreateErrorRdoQuery(_servicesMgr.Object, _logger.Object);
        }

        [Test]
        public void LogError_ShouldTrimMessageAndCreateErrorDto()
        {
            // Arrange
            Error errorDto = new Error()
            {
                Message = new string(Enumerable.Repeat('.', 2001).ToArray()),
                Source = new string(Enumerable.Repeat('.', 256).ToArray())
            };

            // Act
            _sut.LogError(errorDto);

            // Assert
            _errorManager.Verify(x => x.CreateSingleAsync(It.Is<Error>(error=> 
                error.Message.Length <= 2000 && 
                error.Source.Length <= 255)), Times.Once);
        }

        [Test]
        public void LogError_ShouldWriteErrorEventToSystemEvent_WhenErrorManagerFails()
        {
            // Arrange
            Error errorDto = new Error()
            {
                Source = "some source"
            };
            _errorManager.Setup(x => x.CreateSingleAsync(It.IsAny<Error>())).Throws<ServiceException>();

            // Act
            Action action = () => _sut.LogError(errorDto);

            // Assert
            action.ShouldNotThrow();
        }
    }
}