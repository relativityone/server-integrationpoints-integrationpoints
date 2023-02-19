using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.EventHandlers.Commands;
using kCura.IntegrationPoints.EventHandlers.Commands.Context;
using kCura.IntegrationPoints.EventHandlers.Commands.Helpers;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Validators;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Commands
{
    [TestFixture, Category("Unit")]
    public class PreCascadeDeleteIntegrationPointCommandTests : TestBase
    {
        private const int _WORKSPACE_ID = 979307;
        private List<int> _artifactIds;
        private PreCascadeDeleteIntegrationPointCommand _instance;
        private IPreCascadeDeleteEventHandlerValidator _preCascadeDeleteEventHandlerValidator;
        private IDeleteHistoryService _deleteHistoryService;

        public override void SetUp()
        {
            _artifactIds = new List<int>
            {
                463963,
                754377,
                572226
            };

            IEHHelper helper = Substitute.For<IEHHelper>();
            helper.GetActiveCaseID().Returns(_WORKSPACE_ID);
            IEHContext context = new EHContext
            {
                Helper = helper
            };
            _preCascadeDeleteEventHandlerValidator = Substitute.For<IPreCascadeDeleteEventHandlerValidator>();
            _deleteHistoryService = Substitute.For<IDeleteHistoryService>();

            IArtifactsToDelete artifactsToDelete = Substitute.For<IArtifactsToDelete>();
            artifactsToDelete.GetIds().Returns(_artifactIds);

            _instance = new PreCascadeDeleteIntegrationPointCommand(context, _preCascadeDeleteEventHandlerValidator, _deleteHistoryService, artifactsToDelete);
        }

        [Test]
        public void ItShouldValidateDeletion()
        {
            // ACT
            _instance.Execute();

            // ASSERT
            foreach (var artifactId in _artifactIds)
            {
                _preCascadeDeleteEventHandlerValidator.Received(1).Validate(_WORKSPACE_ID, artifactId);
            }
        }

        [Test]
        public void ItShouldDeleteIntegrationPoints()
        {
            // ACT
            _instance.Execute();

            // ASSERT
            foreach (var artifactId in _artifactIds)
            {
                _deleteHistoryService.Received(1).DeleteHistoriesAssociatedWithIP(artifactId);
            }
        }

        [Test]
        public void ItShouldSkipDeletionWhenValidationFailed()
        {
            _preCascadeDeleteEventHandlerValidator.When(x => x.Validate(_WORKSPACE_ID, Arg.Any<int>())).Throw<Exception>();

            // ACT & ASSERT
            Assert.That(() => _instance.Execute(), Throws.Exception);

            _deleteHistoryService.DidNotReceiveWithAnyArgs().DeleteHistoriesAssociatedWithIP(Arg.Any<int>());
        }
    }
}
