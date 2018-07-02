using System;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;
using Relativity.Services.Workspace;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;


namespace kCura.IntegrationPoints.Core.Tests.Validation.RelativityProviderValidator.Parts
{
    [TestFixture]
    public class FederatedInstanceConnectionValidatorTests : TestBase
    {
        private IServicesMgr _servicesMgr;
        private IAPILog _logger;
        private IWorkspaceManager _wokrspaceManager;
        private Core.Managers.IWorkspaceManager _keplerWorkspaceManager;
        public override void SetUp()
        {
            _servicesMgr = Substitute.For<IServicesMgr>();
            _logger = Substitute.For<IAPILog>();
            _keplerWorkspaceManager = Substitute.For<Core.Managers.IWorkspaceManager>();
            _wokrspaceManager = Substitute.For<IWorkspaceManager>();

        }

        [Test]
        public void ValidationShouldFailWhenProxyCreationFails()
        {
            // arrange
            _servicesMgr.CreateProxy<IWorkspaceManager>(Arg.Any<ExecutionIdentity>()).Throws(new Exception("Proxy creation failed"));
            FederatedInstanceConnectionValidator validator = new FederatedInstanceConnectionValidator(_servicesMgr, _keplerWorkspaceManager, _logger);
            
            // act
            ValidationResult result = validator.Validate();

            // assert
            Assert.False(result.IsValid);
        }

        [Test]
        public void ValidationShouldFailWhenWorkspaceRetrievalFails()
        {
            // arrange
            _wokrspaceManager.RetrieveAllActive().Throws(new Exception("Workspace retrieval failed"));
            _servicesMgr.CreateProxy<IWorkspaceManager>(Arg.Any<ExecutionIdentity>()).Returns(_wokrspaceManager);
            FederatedInstanceConnectionValidator validator = new FederatedInstanceConnectionValidator(_servicesMgr, _keplerWorkspaceManager, _logger);

            // act
            ValidationResult result = validator.Validate();

            // assert
            Assert.False(result.IsValid);
        }

        [Test]
        public void ValidationShouldFailWhenWorkspaceRetrievalWithKeplerFails()
        {
            // arrange
            _keplerWorkspaceManager.GetUserWorkspaces().Throws(new Exception("Workspace retrieval failed"));
            _servicesMgr.CreateProxy<IWorkspaceManager>(Arg.Any<ExecutionIdentity>()).Returns(_wokrspaceManager);
            
            FederatedInstanceConnectionValidator validator = new FederatedInstanceConnectionValidator(_servicesMgr, _keplerWorkspaceManager, _logger);

            // act
            ValidationResult result = validator.Validate();

            // assert
            Assert.False(result.IsValid);
        }


        [Test]
        public void ValidationShouldSucceedWhenAllOperationsAreCompleted()
        {
            // arrange
            _servicesMgr.CreateProxy<IWorkspaceManager>(Arg.Any<ExecutionIdentity>()).Returns(_wokrspaceManager);
            FederatedInstanceConnectionValidator validator = new FederatedInstanceConnectionValidator(_servicesMgr, _keplerWorkspaceManager, _logger);

            // act
            ValidationResult result = validator.Validate();

            // assert
            Assert.True(result.IsValid);
        }
    }
}
