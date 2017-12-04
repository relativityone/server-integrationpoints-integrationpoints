using System.Linq;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Core.Validation.Parts;
using kCura.Relativity.Client.DTOs;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Validation
{
	[TestFixture]
	public class ArtifactValidatorTests
	{
		[TestCase(ArtifactTypeNames.Folder)]
		public void ItShouldValidateArtifact(string artifactTypeName)
		{
			// arrange
			var artifactId = 1;
			var workspaceArtifactId = 42;

			var artifact = new Relativity.Client.Artifact { ArtifactID = artifactId };

			var artifactServiceMock = Substitute.For<IArtifactService>();
			artifactServiceMock.GetArtifact(Arg.Any<int>(), Arg.Is<string>(artifactTypeName), artifactId)
				.Returns(artifact);

			var validator = new ArtifactValidator(artifactServiceMock, workspaceArtifactId, artifactTypeName);

			// act
			var actual = validator.Validate(artifactId);

			// assert
			Assert.IsTrue(actual.IsValid);
			Assert.That(actual.Messages.Count(), Is.EqualTo(0));
		}

		[TestCase(ArtifactTypeNames.Folder)]
		public void ItShouldFailValidationForUnknownArtifact(string artifactTypeName)
		{
			// arrange
			var artifactId = 1;
			var workspaceArtifactId = 42;

			var artifactServiceMock = Substitute.For<IArtifactService>();
			artifactServiceMock.GetArtifact(Arg.Any<int>(), Arg.Is<string>(artifactTypeName), artifactId)
				.ReturnsNull();

			var validator = new ArtifactValidator(artifactServiceMock, workspaceArtifactId, artifactTypeName);

			// act
			var actual = validator.Validate(artifactId);

			// assert
			Assert.IsFalse(actual.IsValid);
			Assert.IsTrue(actual.Messages.First().Contains(IntegrationPointProviderValidationMessages.ARTIFACT_NOT_EXIST));
		}
	}
}