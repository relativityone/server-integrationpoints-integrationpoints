using System;
using System.Collections.Generic;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core.Templates;
using NUnit.Framework;
using Relativity.Services.Choice;

namespace kCura.IntegrationPoints.Data.Tests.Integration
{
	[TestFixture]
	public class ChoiceQueryTests : SourceProviderTemplate
	{
		private IChoiceQuery _sut;

		public Guid IntegrationPointsJobTypeGuid = new Guid("e809db5e-5e99-4a75-98a1-26129313a3f5");

		public List<ChoiceRef> ExpectedJobTypeChoiceValues = new List<ChoiceRef>
		{
			new ChoiceRef { Name = "Run" },
			new ChoiceRef { Name = "Scheduled Run" },
			new ChoiceRef { Name = "Retry Errors" }
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
			List<ChoiceRef> result = _sut.GetChoicesOnField(WorkspaceArtifactId, IntegrationPointsJobTypeGuid);

			// Assert
			result.ShouldAllBeEquivalentTo(ExpectedJobTypeChoiceValues, config => config.Excluding(x => x.ArtifactID));
		}
	}
}
