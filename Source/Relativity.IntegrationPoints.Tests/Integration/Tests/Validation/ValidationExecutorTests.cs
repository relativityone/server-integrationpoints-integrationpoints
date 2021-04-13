﻿using System;
using System.Linq;
using FluentAssertions;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Validation
{
	[IdentifiedTestFixture("1B7BBD4A-83D1-4BDC-A9BD-EF7C1C4E1546")]
	public class ValidationExecutorTests : TestsBase
	{
		[IdentifiedTest("E185F16C-6645-4B14-AD30-D26A98D265F5")]
		public void ValidateOnRun_ShouldValidateAndNotThrow()
		{
			// Act & Assert
			ValidateOnOperationShouldNotThrow(
				(executor, context) => executor.ValidateOnRun(context));
		}

		[IdentifiedTest("486B0390-AF71-45CA-AC81-5830D00BF00C")]
		public void ValidateOnSave_ShouldValidateAndNotThrow()
		{
			// Act & Assert
			ValidateOnOperationShouldNotThrow(
				(executor, context) => executor.ValidateOnSave(context));
		}

		[IdentifiedTest("DE07279A-1F14-4F82-9981-B93108B04763")]
		public void ValidateOnStop_ShouldValidateAndNotThrow()
		{
			// Act & Assert
			ValidateOnOperationShouldNotThrow(
				(executor, context) => executor.ValidateOnStop(context));
		}

		[IdentifiedTest("69E2F16A-7B05-4C63-B0CC-825B4BBB5F32")]
		public void ValidateOnProfile_ShouldValidate()
		{
			// Arrange
			ValidationContext context = PrepareValidationContext();

			IValidationExecutor sut = PrepareSut();

			// Act 
			ValidationResult result = sut.ValidateOnProfile(context);

			// Assert
			result.IsValid.Should().BeTrue();
			result.Messages.Should().BeEmpty();
		}

		private void ValidateOnOperationShouldNotThrow(Action<IValidationExecutor, ValidationContext> validateAction)
		{
			// Arrange
			ValidationContext context = PrepareValidationContext();

			IValidationExecutor sut = PrepareSut();

			// Act 
			Action validation = () => validateAction(sut, context);

			// Assert
			validation.ShouldNotThrow();
		}

		private IValidationExecutor PrepareSut()
		{
			return Container.Resolve<IValidationExecutor>();
		}

		private ValidationContext PrepareValidationContext()
		{
			WorkspaceTest destinationWorkspace = FakeRelativityInstance.Helpers.WorkspaceHelper.CreateWorkspace();

			IntegrationPointTest integrationPoint =
				SourceWorkspace.Helpers.IntegrationPointHelper.CreateSavedSearchIntegrationPoint(destinationWorkspace);

			IntegrationPointTypeTest integrationPointType = SourceWorkspace.IntegrationPointTypes.Single(x => x.ArtifactId == integrationPoint.Type);

			SourceProviderTest sourceProvider = SourceWorkspace.SourceProviders.Single(x => x.ArtifactId == integrationPoint.SourceProvider);

			DestinationProviderTest destinationProvider = SourceWorkspace.DestinationProviders.Single(x => x.ArtifactId == integrationPoint.DestinationProvider);

			ValidationContext context = new ValidationContext
			{
				Model = IntegrationPointModel.FromIntegrationPoint(integrationPoint.ToRdo()),
				SourceProvider = sourceProvider.ToRdo(),
				DestinationProvider = destinationProvider.ToRdo(),
				IntegrationPointType = integrationPointType.ToRdo(),
				ObjectTypeGuid = ObjectTypeGuids.IntegrationPointGuid,
				UserId = User.ArtifactId
			};

			return context;
		}
	}
}
