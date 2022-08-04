using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Validation.RelativityProviderValidator
{
    [TestFixture, Category("Unit")]
    public class RelativityProviderDestinationWorkspaceExistenceValidatorTests : TestBase
    {
        private IWorkspaceManager _workspaceManager;

        private const int _DESTINATION_WORKSPACE_ID = 349234;
        private const int _FEDERATED_INSTANCE_ID = 432943;

        [SetUp]
        public override void SetUp()
        {
            _workspaceManager = Substitute.For<IWorkspaceManager>();
        }

        [Test]
        public void ItShouldValidateDestinationWorkspaceExistence(
            [Values(true, false)] bool workspaceExists,
            [Values(true, false)] bool isFederatedInstance)
        {
            // arrange
            SourceConfiguration sourceConfiguration = new SourceConfiguration
            {
                TargetWorkspaceArtifactId = _DESTINATION_WORKSPACE_ID,
                FederatedInstanceArtifactId = isFederatedInstance ? (int?) _FEDERATED_INSTANCE_ID : null
            };
            _workspaceManager.WorkspaceExists(_DESTINATION_WORKSPACE_ID).Returns(workspaceExists);
            var destinationWorkspaceExistenceValidator = new RelativityProviderDestinationWorkspaceExistenceValidator(_workspaceManager);

            // act
            ValidationResult validationResult = destinationWorkspaceExistenceValidator.Validate(sourceConfiguration);

            // assert
            _workspaceManager.Received(1).WorkspaceExists(_DESTINATION_WORKSPACE_ID);
            Assert.AreEqual(workspaceExists, validationResult.IsValid);
            Assert.AreEqual(workspaceExists ? 0 : 1, validationResult.Messages.Count());
            if (!workspaceExists)
            {
                ValidationMessage actualMessage = validationResult.Messages.First();
                ValidationMessage expectedMessage = isFederatedInstance
                    ? ValidationMessages.FederatedInstanceDestinationWorkspaceNotAvailable
                    : ValidationMessages.DestinationWorkspaceNotAvailable;
                Assert.AreEqual(expectedMessage.ShortMessage, actualMessage.ShortMessage);
                Assert.AreEqual(expectedMessage.ErrorCode, actualMessage.ErrorCode);
            }
        }
    }
}
