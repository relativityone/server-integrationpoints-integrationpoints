using System;
using kCura.IntegrationPoints.Services;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Templates;
using Newtonsoft.Json;
using NUnit.Framework;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Integration
{
	public class ViewErrors : WorkspaceDependentTemplate
	{
		public ViewErrors() : base("ViewErrorsSource", "ViewErrorsSourceDestination")
		{
		}

		[Test]
		[Explicit]
		public void ExpectDisabledViewErrorsLinkOnIntegrationPointCreation()
		{
			//Arrange
			UserModel user = new UserModel
			{
				EmailAddress = SharedVariables.RelativityUserName,
				Password = SharedVariables.RelativityPassword
			};

			string response = kCura.IntegrationPoint.Tests.Core.IntegrationPoint.CreateIntegrationPoint(Guid.NewGuid().ToString(),
				SourceWorkspaceArtifactId,
				TargetWorkspaceArtifactId,
				1039795,
				FieldOverlayBehavior.UseFieldSettings,
				ImportOverwriteMode.AppendOverlay,
				false,
				false,
				user);

			IntegrationPointModel integrationPoint = JsonConvert.DeserializeObject<IntegrationPointModel>(response);

			//Act
			bool ranIntegrationPoint = kCura.IntegrationPoint.Tests.Core.IntegrationPoint.RunIntegrationPoint(SourceWorkspaceArtifactId, integrationPoint.ArtifactId, user);

			//Assert
			Assert.IsTrue(ranIntegrationPoint);


		}
	}
}
