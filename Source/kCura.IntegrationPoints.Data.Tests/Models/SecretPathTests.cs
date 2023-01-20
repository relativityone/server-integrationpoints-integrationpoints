using System;
using FluentAssertions;
using kCura.IntegrationPoints.Data.Models;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Data.Tests.Models
{
    [TestFixture, Category("Unit")]
    public class SecretPathTests
    {
        private const int _WORKSPACE_ID = 2002;

        [Test]
        public void ForIntegrationPointSecret_ShouldReturnSecretPathWhenCorrectSecretIDPassed()
        {
            //arrange
            const int integrationPointID = 1001;
            string secretID = "9d26cf1b-3a4e-46fa-a88f-0318f61a796f";

            //act
            SecretPath secretPath = SecretPath.ForIntegrationPointSecret(
                _WORKSPACE_ID,
                integrationPointID,
                secretID
            );

            //assert
            AssertSecretPath(
                secretPath,
                _WORKSPACE_ID,
                integrationPointID,
                secretID
            );
        }

        [Test]
        public void ForAllSecretsInIntegrationPoint_ShouldReturnCorrectSecretPath()
        {
            //arrange
            const int integrationPointID = 2002;

            //act
            SecretPath secretPath = SecretPath.ForAllSecretsInIntegrationPoint(
                _WORKSPACE_ID,
                integrationPointID
            );

            //assert
            AssertSecretPath(
                secretPath,
                _WORKSPACE_ID,
                integrationPointID
            );
        }

        [Test]
        public void ForAllSecretsInWorkspace_ShouldReturnCorrectSecretPath()
        {
            //act
            SecretPath secretPath = SecretPath.ForAllSecretsInWorkspace(_WORKSPACE_ID);

            //assert
            AssertSecretPath(
                secretPath,
                _WORKSPACE_ID
            );
        }

        [Test]
        public void ForAllSecretsInAllWorkspaces_ShouldReturnCorrectSecretPath()
        {
            //act
            SecretPath secretPath = SecretPath.ForAllSecretsInAllWorkspaces();

            //assert
            AssertSecretPath(secretPath);
        }

        [Test]
        [TestCase(-1, 1, "360a4f5a-c035-4e9d-9af1-56dddd86b64e")]
        [TestCase(1, -1, "360a4f5a-c035-4e9d-9af1-56dddd86b64e")]
        [TestCase(1, 1, "invalid guid")]
        public void ForIntegrationPointSecret_ShouldThrowWhenInvalidPathDetected(
            int workspaceID,
            int integrationPointID,
            string secretID)
        {
            //act
            Action action = () => SecretPath.ForIntegrationPointSecret(
                workspaceID,
                integrationPointID,
                secretID
            );

            //assert
            AssertInvalidPathExceptionThrown(action, workspaceID, integrationPointID, secretIdIsInvalidGuid: !Guid.TryParse(secretID, out Guid secretIDGuid));
        }

        [Test]
        [TestCase(-1, 1)]
        [TestCase(1, -1)]
        public void ForAllSecretsInIntegrationPoint_ShouldThrowWhenInvalidPathDetected(
            int workspaceID,
            int integrationPointID)
        {
            //act
            const string secretID = "360a4f5a-c035-4e9d-9af1-56dddd86b64e";
            Action action = () => SecretPath.ForIntegrationPointSecret(
                workspaceID,
                integrationPointID,
                secretID
            );

            //assert
            AssertInvalidPathExceptionThrown(action, workspaceID, integrationPointID, secretIdIsInvalidGuid: false);
        }

        [Test]
        public void ForAllSecretsInWorkspace_ShouldThrowWhenNegativeValuePassedAsWorkspaceID()
        {
            //act
            const string secretID = "360a4f5a-c035-4e9d-9af1-56dddd86b64e";
            const int integrationPointID = 2002;
            const int workspaceID = -1;
            Action action = () => SecretPath.ForIntegrationPointSecret(
                workspaceID,
                integrationPointID,
                secretID
            );

            //assert
            AssertInvalidPathExceptionThrown(action, workspaceID, integrationPointID, secretIdIsInvalidGuid: false);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("    ")]
        public void ForIntegrationPointSecret_ShouldThrowWhenNullOrWhitespacePassedAsSecretID(string secretID)
        {
            //act
            const int integrationPointID = 2002;
            Action action = () => SecretPath.ForIntegrationPointSecret(
                _WORKSPACE_ID,
                integrationPointID,
                secretID
            );

            //assert
            action
                .ShouldThrow<ArgumentException>()
                .WithMessage("Invalid secret path. SecretID cannot be null or whitespace.");
        }

        private void AssertInvalidPathExceptionThrown(Action action, int workspaceID, int integrationPointID, bool secretIdIsInvalidGuid)
        {
            action
                .ShouldThrow<ArgumentException>()
                .WithMessage(
                    $"Invalid secret path. WorkspaceID: {workspaceID}, IntegrationPointID: {integrationPointID}, SecretID is invalid GUID: {secretIdIsInvalidGuid}");
        }

        private void AssertSecretPath(
            SecretPath actualSecretPath,
            int? workspaceID = null,
            int? integrationPointID = null,
            string secretID = null)
        {
            string expectedPath = string.Empty;

            if (workspaceID != null)
            {
                actualSecretPath.WorkspaceID.Should().Be(workspaceID);
                expectedPath = $"{expectedPath}/{workspaceID}";
            }

            if (integrationPointID != null)
            {
                actualSecretPath.IntegrationPointID.Should().Be(integrationPointID);
                expectedPath = $"{expectedPath}/{integrationPointID}";
            }

            if (secretID != null)
            {
                actualSecretPath.SecretID.Should().Be(secretID);
                expectedPath = $"{expectedPath}/{secretID}";
            }

            actualSecretPath.ToString().Should().Be(expectedPath);
        }
    }
}
