using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.IntegrationPoints.FieldsMapping;
using Relativity.IntegrationPoints.FieldsMapping.FieldClassifiers;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Web.Tests.Controllers.API.FieldMappings.FieldsClassifiers
{
	[TestFixture]
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
			const string openToAssociations = "Open To Associations";

			const string openToAssociationsEnabledFieldName = "Open to associations - enabled";
			const string openToAssociationsDisabledFieldName = "Open to associations - disabled";
			const string withoutOpenToAssociationsFieldName = "Without open to associations";

			List<RelativityObject> allFields = new List<RelativityObject>()
			{
				new RelativityObject()
				{
					Name = openToAssociationsEnabledFieldName,
					FieldValues = new List<FieldValuePair>()
					{
						new FieldValuePair()
						{
							Field = new Field()
							{
								Name = openToAssociations
							},
							Value = true
						}
					}
				},
				new RelativityObject()
				{
					Name = openToAssociationsDisabledFieldName,
					FieldValues = new List<FieldValuePair>()
					{
						new FieldValuePair()
						{
							Field = new Field()
							{
								Name = openToAssociations
							},
							Value = false
						}
					}
				},
				new RelativityObject()
				{
					Name = withoutOpenToAssociationsFieldName,
					FieldValues = new List<FieldValuePair>()
				}
			};

			// Act
			List<FieldClassificationResult> classifiedFields = (await _sut.ClassifyAsync(allFields, 0).ConfigureAwait(false)).ToList();

			// Assert
			classifiedFields.Count.Should().Be(1);
			FieldClassificationResult classifiedField = classifiedFields.First();
			classifiedField.Name.Should().Be(openToAssociationsEnabledFieldName);
			classifiedField.ClassificationLevel.Should().Be(ClassificationLevel.ShowToUser);
		}
	}
}