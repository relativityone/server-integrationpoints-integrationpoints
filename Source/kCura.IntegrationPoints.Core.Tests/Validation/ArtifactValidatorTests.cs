using System.Linq;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Core.Validation.Parts;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Tests.Validation
{
	[TestFixture, Category("Unit")]
	public class ArtifactValidatorTests
	{
		[TestCase("Folder")]
		public void ItShouldValidateArtifact(string artifactTypeName)
		{
			// arrange
			var artifactId = 1;
			var workspaceArtifactId = 42;

			var artifact = new RelativityObject
			{
				ArtifactID = artifactId
			};

			var artifactServiceMock = Substitute.For<IArtifactService>();
			artifactServiceMock.GetArtifact(Arg.Any<int>(), Arg.Is<string>(artifactTypeName), artifactId)
				.Returns(artifact);

			var validator = new ArtifactValidator(artifactServiceMock, workspaceArtifactId, artifactTypeName);

			// act
			var actual = validator.Validate(artifactId);

			// assert
			Assert.IsTrue(actual.IsValid);
			Assert.That(actual.MessageTexts.Count(), Is.EqualTo(0));
		}

		[TestCase("Folder")]
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
			Assert.IsTrue(actual.MessageTexts.First().Contains(IntegrationPointProviderValidationMessages.ARTIFACT_NOT_EXIST));
		}
	}
}