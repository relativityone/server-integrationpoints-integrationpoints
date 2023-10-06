﻿using System;
using FluentAssertions;
using kCura.IntegrationPoints.Core.Validation;
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
            IntegrationPointFake integrationPoint = PrepareIntegrationPoint();

            IAgentValidator sut = Container.Resolve<IAgentValidator>();

            // Act
            Action validation = () => sut.Validate(integrationPoint.ToDto(), User.ArtifactId);

            // Assert
            validation.ShouldNotThrow();
        }

        private IntegrationPointFake PrepareIntegrationPoint()
        {
            WorkspaceFake destinationWorkspace = FakeRelativityInstance.Helpers.WorkspaceHelper.CreateWorkspace();

            IntegrationPointFake integrationPoint =
                SourceWorkspace.Helpers.IntegrationPointHelper.CreateSavedSearchSyncIntegrationPoint(destinationWorkspace);

            return integrationPoint;
        }
    }
}
