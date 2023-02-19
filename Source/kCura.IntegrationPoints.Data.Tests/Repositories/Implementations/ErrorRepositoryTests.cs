using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Domain.Models;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Error;
using Relativity.Services.Exceptions;

namespace kCura.IntegrationPoints.Data.Tests.Repositories.Implementations
{
    [TestFixture]
    public class ErrorRepositoryTests
    {
        private Mock<IErrorManager> _errorManager;
        private Mock<IHelper> _helper;
        private Mock<IAPILog> _logger;
        private ErrorRepository _sut;

        [SetUp]
        public void SetUp()
        {
            _errorManager = new Mock<IErrorManager>();
            _helper = new Mock<IHelper>();
            _helper.Setup(x => x.GetServicesManager().CreateProxy<IErrorManager>(It.IsAny<ExecutionIdentity>())).Returns(_errorManager.Object);
            _logger = new Mock<IAPILog>();
            _helper.Setup(x => x.GetLoggerFactory().GetLogger().ForContext<ErrorRepository>()).Returns(_logger.Object);

            _sut = new ErrorRepository(_helper.Object);
        }

        [Test]
        public void Create_ShouldCreateAllErrors()
        {
            // Arrange
            List<ErrorDTO> errors = Enumerable.Range(0, 2)
                .Select(x => new ErrorDTO())
                .ToList();

            // Act
            _sut.Create(errors);

            // Assert
            _errorManager.Verify(x => x.CreateSingleAsync(It.IsAny<Error>()), Times.Exactly(errors.Count));
        }

        [Test]
        public void Create_ShouldLogError_WhenErrorManagerFails()
        {
            // Arrange
            _errorManager.Setup(x => x.CreateSingleAsync(It.IsAny<Error>())).Throws<ServiceException>();

            // Act
            System.Action action = () => _sut.Create(new[] { new ErrorDTO()});

            // Assert
            action.ShouldNotThrow();
            _logger.Verify(x => x.LogError(It.IsAny<ServiceException>(), It.IsAny<string>(), It.IsAny<object>()));
        }
    }
}
