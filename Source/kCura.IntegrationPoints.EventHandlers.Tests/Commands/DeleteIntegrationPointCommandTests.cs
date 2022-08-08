using System;
using kCura.EventHandler;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.EventHandlers.Commands;
using kCura.IntegrationPoints.EventHandlers.Commands.Context;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Commands
{
    [TestFixture, Category("Unit")]
    public class DeleteIntegrationPointCommandTests : TestBase
    {
        private const int _ARTIFACT_ID = 757404;
        private const int _WORKSPACE_ID = 498941;

        private DeleteIntegrationPointCommand _instance;
        private ICorrespondingJobDelete _correspondingJobDelete;
        private IIntegrationPointSecretDelete _integrationPointSecretDelete;

        public override void SetUp()
        {
            _correspondingJobDelete = Substitute.For<ICorrespondingJobDelete>();
            _integrationPointSecretDelete = Substitute.For<IIntegrationPointSecretDelete>();

            var helper = Substitute.For<IEHHelper>();
            helper.GetActiveCaseID().Returns(_WORKSPACE_ID);

            IEHContext context = new EHContext
            {
                ActiveArtifact = new Artifact(_ARTIFACT_ID, null, 1, "", false, null),
                Helper = helper
            };

            _instance = new DeleteIntegrationPointCommand(_correspondingJobDelete, _integrationPointSecretDelete, context);
        }

        [Test]
        public void ItShouldDeleteJobsAndSecrets()
        {
            // ACT
            _instance.Execute();

            // ASSERT
            _correspondingJobDelete.Received(1).DeleteCorrespondingJob(_WORKSPACE_ID, _ARTIFACT_ID);
            _integrationPointSecretDelete.DeleteSecret(_ARTIFACT_ID);
        }

        [Test]
        public void ItShouldSkipSecretDeletionAfterDeletingJobsFailed()
        {
            _correspondingJobDelete.When(x => x.DeleteCorrespondingJob(_WORKSPACE_ID, _ARTIFACT_ID)).Throw<Exception>();

            // ACT & ASSERT
            Assert.That(() => _instance.Execute(), Throws.Exception);

            _integrationPointSecretDelete.DidNotReceive().DeleteSecret(_ARTIFACT_ID);
        }
    }
}