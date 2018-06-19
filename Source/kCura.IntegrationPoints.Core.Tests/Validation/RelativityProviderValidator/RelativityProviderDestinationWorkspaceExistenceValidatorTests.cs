using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Validation.RelativityProviderValidator
{
	[TestFixture]
	public class RelativityProviderDestinationWorkspaceExistenceValidatorTests : TestBase
	{
		private IWorkspaceManager _workspaceManager;

		private const int _DESTINATION_WORKSPACE_ID = 349234;

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
			_workspaceManager.WorkspaceExists(_DESTINATION_WORKSPACE_ID).Returns(workspaceExists);
			var destinationWorkspaceExistenceValidator = new RelativityProviderDestinationWorkspaceExistenceValidator(_workspaceManager);

			// act
			ValidationResult validationResult = destinationWorkspaceExistenceValidator.Validate(_DESTINATION_WORKSPACE_ID, isFederatedInstance);

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
