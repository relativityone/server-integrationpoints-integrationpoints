using System;
using System.Collections.Generic;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core.Templates;
using NUnit.Framework;
using Choice = kCura.Relativity.Client.DTOs.Choice;

namespace kCura.IntegrationPoints.Data.Tests.Integration
{
	[TestFixture]
	public class ChoiceQueryTests : SourceProviderTemplate
	{
		private IChoiceQuery _sut;

		public Guid IntegrationPointsJobTypeGuid = new Guid("e809db5e-5e99-4a75-98a1-26129313a3f5");

		public List<Choice> ExpectedJobTypeChoiceValues = new List<Choice>
		{
			new Choice
			{
				Name = "Run"
			},
			new Choice
			{
				Name = "Scheduled Run"
			},
			new Choice
			{
				Name = "Retry Errors"
			}
		};

		public ChoiceQueryTests() : base("ChoiceQueryTests Workspace")
		{
		}

		[SetUp]
		public void SetUp()
		{
			_sut = Container.Resolve<IChoiceQuery>();
		}

		[Test]
		public void GetChoicesOnField_ShouldReturnExpectedChoiceValues_WhenChoiceGuidIsPassed()
		{
			// Arrange & Act
			List<Choice> result = _sut.GetChoicesOnField(WorkspaceArtifactId, IntegrationPointsJobTypeGuid);

			// Assert
			AssertChoiceValues(result);
		}

		private void AssertChoiceValues(List<Choice> choiceValues)
		{
			choiceValues.ShouldAllBeEquivalentTo(ExpectedJobTypeChoiceValues, config =>
			{
				config.Excluding(x => x.ArtifactID);

				return config;
			});
		}
	}
}
