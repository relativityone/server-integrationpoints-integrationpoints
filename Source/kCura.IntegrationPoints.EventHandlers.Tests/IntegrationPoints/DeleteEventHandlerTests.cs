using System;
using kCura.EventHandler;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Tests.IntegrationPoints
{
	[TestFixture]
	public class DeleteEventHandlerTests : TestBase
	{
		private const int _WORKSPACE_ID = 631142;
		private const int _INTEGRATION_POINT_ID = 804272;

		private IAPILog _logger;
		private IEHHelper _helper;
		private ICorrespondingJobDelete _correspondingJobDelete;
		private IIntegrationPointSecretDelete _integrationPointSecretDelete;
		private DeleteEventHandler _deleteEventHandler;

		public override void SetUp()
		{
			_logger = Substitute.For<IAPILog>();
			_helper = Substitute.For<IEHHelper>();
			_correspondingJobDelete = Substitute.For<ICorrespondingJobDelete>();
			_integrationPointSecretDelete = Substitute.For<IIntegrationPointSecretDelete>();

			_helper.GetLoggerFactory().GetLogger().ForContext<DeleteEventHandler>().Returns(_logger);
			_helper.GetActiveCaseID().Returns(_WORKSPACE_ID);

			_deleteEventHandler = new DeleteEventHandler
			{
				Helper = _helper,
				CorrespondingJobDelete = _correspondingJobDelete,
				IntegrationPointSecretDelete = _integrationPointSecretDelete,
				ActiveArtifact = new Artifact(_INTEGRATION_POINT_ID, null, 765, "", false, null)
			};
		}

		[Test]
		public void GoldWorkflow()
		{
			var response = _deleteEventHandler.Execute();

			Assert.That(response.Success, Is.True);
			Assert.That(response.Message, Is.EqualTo("Integration Point successfully deleted."));

			_correspondingJobDelete.Received(1).DeleteCorrespondingJob(_WORKSPACE_ID, _INTEGRATION_POINT_ID);
			_integrationPointSecretDelete.Received(1).DeleteSecret(_INTEGRATION_POINT_ID);
		}

		[Test]
		public void ItShouldCatchExecutionExceptionAndLogIt_CorrespondingJobDelete()
		{
			var expectedException = new Exception("message");

			_correspondingJobDelete.When(x => x.DeleteCorrespondingJob(_WORKSPACE_ID, _INTEGRATION_POINT_ID)).Do(x => { throw expectedException; });

			var response = _deleteEventHandler.Execute();

			Assert.That(response.Success, Is.False);
			Assert.That(response.Exception.Message, Is.EqualTo(expectedException.Message));
			Assert.That(response.Message, Is.EqualTo($"Failed to delete corresponding job(s). Error: {expectedException.Message}"));

			_logger.Received(1).LogError(expectedException, "Failed to delete corresponding job(s).");
		}

		[Test]
		public void ItShouldCatchExecutionExceptionAndLogIt_IntegrationPointSecretDelete()
		{
			var expectedException = new Exception("message");

			_integrationPointSecretDelete.When(x => x.DeleteSecret(_INTEGRATION_POINT_ID)).Do(x => { throw expectedException; });

			var response = _deleteEventHandler.Execute();

			Assert.That(response.Success, Is.False);
			Assert.That(response.Exception.Message, Is.EqualTo(expectedException.Message));
			Assert.That(response.Message, Is.EqualTo($"Failed to delete corresponding secret. Error: {expectedException.Message}"));

			_logger.Received(1).LogError(expectedException, "Failed to delete corresponding secret.");
		}
	}
}