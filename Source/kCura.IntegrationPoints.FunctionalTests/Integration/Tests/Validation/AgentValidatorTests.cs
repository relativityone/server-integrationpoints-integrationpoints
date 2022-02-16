using System;
using FluentAssertions;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Validation
{
	public class AgentValidatorTests : TestsBase
	{
		[IdentifiedTest("0895F31D-E64B-46C7-ABAC-52ECF904CD79")]
		public void Validate_ShouldNotThrow()
		{
			// Arrange
			IntegrationPoint integrationPoint = PrepareIntegrationPoint();

			IAgentValidator sut = Container.Resolve<IAgentValidator>();

			// Act
			Action validation = () => sut.Validate(integrationPoint, User.ArtifactId);

			// Assert
			validation.ShouldNotThrow();
		}

		private IntegrationPoint PrepareIntegrationPoint()
		{
			WorkspaceTest destinationWorkspace = FakeRelativityInstance.Helpers.WorkspaceHelper.CreateWorkspace();

			IntegrationPointTest integrationPoint =
				SourceWorkspace.Helpers.IntegrationPointHelper.CreateSavedSearchSyncIntegrationPoint(destinationWorkspace);

			return integrationPoint.ToRdo();
		}
	}
}
