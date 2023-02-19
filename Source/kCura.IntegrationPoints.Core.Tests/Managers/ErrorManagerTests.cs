using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Managers
{
    [TestFixture, Category("Unit")]
    public class ErrorManagerTests : TestBase
    {
        private IErrorManager _errorManager;
        private IRepositoryFactory _repositoryFactory;
        private IErrorRepository _errorRepository;
        private const int _WORKSPACE_ID = 1234567;

        [SetUp]
        public override void SetUp()
        {
            _repositoryFactory = Substitute.For<IRepositoryFactory>();
            _errorRepository = Substitute.For<IErrorRepository>();
            _errorManager = new ErrorManager(_repositoryFactory);

            _repositoryFactory.GetErrorRepository().Returns(_errorRepository);
        }

        [Test]
        public void Create_GoldFlow()
        {
            // ARRANGE
            string message = " This is an error";
            List<ErrorDTO> errors = new List<ErrorDTO>
            {
                new ErrorDTO
                {
                    Message = message,
                    Source = Core.Constants.IntegrationPoints.APPLICATION_NAME,
                    WorkspaceId = _WORKSPACE_ID
                }
            };

            _errorRepository.Create(Arg.Is<IEnumerable<ErrorDTO>>(x => x.Equals(errors)));

            // ACT
            _errorManager.Create(errors);

            // ASSERT
            _errorRepository.Received(1).Create(Arg.Is<IEnumerable<ErrorDTO>>(x => x.Equals(errors)));
        }
    }
}
