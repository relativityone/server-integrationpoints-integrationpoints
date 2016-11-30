using System;
using System.Linq;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Validation.RelativityProviderValidator
{
	[TestFixture]
	public class WorkspaceValidatorTests
	{
		[Test]
		public void ItShouldValidateWorkspace()
		{
			// arrange
			var workspaceArtifactId = 42;
			var workspaceName = "workspace";
			var workspacePrefix = "prefix";

			WorkspaceDTO workspaceDTO = new WorkspaceDTO
			{
				ArtifactId = workspaceArtifactId,
				Name = workspaceName
			};

			var workspaceRepositoryMock = Substitute.For<IWorkspaceRepository>();
			workspaceRepositoryMock.Retrieve(Arg.Any<int>())
				.Returns(workspaceDTO);

			var validator = new RelativityProviderWorkspaceValidator(workspaceRepositoryMock, workspacePrefix);

			// act
			ValidationResult actual = validator.Validate(workspaceArtifactId);

			// assert
			Assert.IsTrue(actual.IsValid);
			Assert.That(actual.Messages.Count(), Is.EqualTo(0));
		}

		[Test]
		public void ItShouldFailValidationForNonExistantWorkspace()
		{
			// arrange
			var workspaceArtifactId = 42;
			var workspacePrefix = "prefix";

			var workspaceRepositoryMock = Substitute.For<IWorkspaceRepository>();
			workspaceRepositoryMock.Retrieve(Arg.Any<int>())
				.Returns(x => { throw new Exception("Thou shall not pass!"); });

			var validator = new RelativityProviderWorkspaceValidator(workspaceRepositoryMock, workspacePrefix);

			// act
			ValidationResult actual = validator.Validate(workspaceArtifactId);

			// assert
			Assert.IsFalse(actual.IsValid);
			Assert.IsTrue(actual.Messages.First().Contains(IntegrationPointProviderValidationMessages.WORKSPACE_NOT_EXIST));
			Assert.IsTrue(actual.Messages.First().Contains(workspacePrefix));
		}

		[TestCase(";")]
		[TestCase("NameOf;Workspace")]
		[TestCase("NameOfWorkspace;")]
		public void ItShouldFailValidationForInvalidName(string workspaceName)
		{
			// arrange
			var workspaceArtifactId = 42;
			var workspacePrefix = "prefix";

			WorkspaceDTO workspaceDTO = new WorkspaceDTO
			{
				Name = workspaceName
			};

			var workspaceRepositoryMock = Substitute.For<IWorkspaceRepository>();
			workspaceRepositoryMock.Retrieve(Arg.Any<int>())
				.Returns(workspaceDTO);

			var validator = new RelativityProviderWorkspaceValidator(workspaceRepositoryMock, workspacePrefix);

			// act
			ValidationResult actual = validator.Validate(workspaceArtifactId);

			// assert
			Assert.IsFalse(actual.IsValid);
			Assert.IsTrue(actual.Messages.First().Contains(RelativityProviderValidationMessages.WORKSPACE_INVALID_NAME));
			Assert.IsTrue(actual.Messages.First().Contains(workspacePrefix));
		}
	}
}