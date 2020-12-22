using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Services;
using NUnit.Framework;
using Relativity;

namespace kCura.IntegrationPoints.Core.Tests.Integration.Services
{
	[TestFixture]
	public class ChoiceServiceTests : SourceProviderTemplate
	{
		private IChoiceService _sut;

		public ChoiceServiceTests() : base(1017774)
		{
		}

		[SetUp]
		public void SetUp()
		{
			_sut = Container.Resolve<IChoiceService>();
		}

		[Test]
		public void GetChoicesOnField_ShouldReturnExpectedChoiceValues_WhenChoiceArtifactIdIsPassed()
		{
			// Arrange
			// Act
			var result = _sut.GetChoiceFields(WorkspaceArtifactId, (int) ArtifactType.Document);
			// Assert
		}
	}
}
