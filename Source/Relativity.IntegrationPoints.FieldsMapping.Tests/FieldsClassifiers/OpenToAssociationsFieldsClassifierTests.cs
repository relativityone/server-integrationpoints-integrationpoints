using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.IntegrationPoints.FieldsMapping.FieldClassifiers;

namespace Relativity.IntegrationPoints.FieldsMapping.Tests.FieldsClassifiers
{
	[TestFixture, Category("Unit")]
	public class OpenToAssociationsFieldsClassifierTests
	{
		private OpenToAssociationsFieldsClassifier _sut;

		[SetUp]
		public void SetUp()
		{
			_sut = new OpenToAssociationsFieldsClassifier();
		}

		[Test]
		public async Task ClassifyAsync_ShouldClassifyFieldsThatHaveOpenToAssociationsEnabled()
		{
			// Arrange
			const string openToAssociationsEnabledFieldName = "Open to associations - enabled";
			const string openToAssociationsDisabledFieldName = "Open to associations - disabled";
			const string withoutOpenToAssociationsFieldName = "Without open to associations";

			List<FieldInfo> allFields = new List<FieldInfo>()
			{
				new FieldInfo(fieldIdentifier: "1", name: openToAssociationsEnabledFieldName, type: "Fixed-Length Text(250)")
				{
					OpenToAssociations = true
				},
				new FieldInfo(fieldIdentifier: "1", name: openToAssociationsDisabledFieldName, type: "Fixed-Length Text(250)")
				{
					OpenToAssociations = false
				},
				new FieldInfo(fieldIdentifier: "1", name: withoutOpenToAssociationsFieldName, type: "Fixed-Length Text(250)")
			};

			// Act
			List<FieldClassificationResult> classifiedFields = (await _sut.ClassifyAsync(allFields, 0).ConfigureAwait(false)).ToList();

			// Assert
			classifiedFields.Count.Should().Be(1);
			FieldClassificationResult classifiedField = classifiedFields.First();
			classifiedField.FieldInfo.Name.Should().Be(openToAssociationsEnabledFieldName);
			classifiedField.ClassificationLevel.Should().Be(ClassificationLevel.ShowToUser);
		}
	}
}