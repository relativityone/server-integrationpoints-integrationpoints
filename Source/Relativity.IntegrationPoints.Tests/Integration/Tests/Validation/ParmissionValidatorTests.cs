using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Core.Contracts;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation.Parts;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Validation
{
	[IdentifiedTestFixture("07C8125E-6420-4DEE-9D59-309AB26AD33E")]
	public class PermissionValidatorTests : TestsBase
	{
		[IdentifiedTest("EA5F1E35-204F-44C4-B499-D535337DB61E")]
		public void ImportPermissionValidator_ShouldValidate()
		{
			// Arrange
			object value = new IntegrationPointProviderValidationModel()
			{
				DestinationConfiguration = Serializer.Serialize(new DestinationConfiguration
				{
					ArtifactTypeId = (int)ArtifactType.Document
				})
			};

			// Act & Assert
			ValidatePermissions<ImportPermissionValidator>(value);
		}

		[IdentifiedTest("6F0ED21B-1E2A-4E87-AC7A-80E1CC639993")]
		public void PermissionValidator_ShouldValidate()
		{
			// Arrange
			IntegrationPointTest integrationPoint = HelperManager.IntegrationPointHelper
				.CreateIntegrationPointWithFakeProviders(SourceWorkspace);

			IntegrationPointProviderValidationModel value = new IntegrationPointProviderValidationModel()
			{
				ArtifactId = integrationPoint.ArtifactId,
				SourceProviderArtifactId = integrationPoint.SourceProvider.GetValueOrDefault(),
				ObjectTypeGuid = ObjectTypeGuids.IntegrationPointGuid,
				DestinationConfiguration = Serializer.Serialize(new DestinationConfiguration
				{
					ArtifactTypeId = (int)ArtifactType.Document
				})
			};

			// Act & Assert
			ValidatePermissions<PermissionValidator>(value);
		}

		private void ValidatePermissions<T>(object value)
			where T: IPermissionValidator
		{
			IPermissionValidator sut = Container.Resolve<IPermissionValidator>(typeof(T).Name);

			ValidationResult result = sut.Validate(value);

			result.IsValid.Should().BeTrue();
			result.Messages.Should().BeEmpty();
		}
	}
}
