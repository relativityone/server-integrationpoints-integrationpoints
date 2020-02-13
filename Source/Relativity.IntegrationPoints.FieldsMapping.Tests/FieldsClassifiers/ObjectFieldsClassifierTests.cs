using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.IntegrationPoints.FieldsMapping.FieldClassifiers;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.FieldsMapping.Tests.FieldsClassifiers
{
	[TestFixture, Category("Unit")]
	public class ObjectFieldsClassifierTests
	{
		private const string ApiDoesNotSupportAllObjectTypes = "API does not support all object types.";
		private ObjectFieldsClassifier _sut;

		[SetUp]
		public void SetUp()
		{
			_sut = new ObjectFieldsClassifier();
		}

		[Test]
		public async Task ClassifyAsync_ShouldProperlyClassifySingleObjectFields()
		{
			// Arrange 

			var fields = new List<RelativityObject>
			{
				new RelativityObject
				{
					FieldValues = new List<FieldValuePair>
					{
						new FieldValuePair
						{
							Field = new Field
							{
								Name = "Field Type"
							},
							Value = "Single Object"
						}
					}
				}
			};

			// Act
			FieldClassificationResult[] classifications = (await _sut.ClassifyAsync(fields, 0).ConfigureAwait(false)).ToArray();

			// Assert
			classifications.Length.Should().Be(1);

			classifications[0].ClassificationLevel.Should().Be(ClassificationLevel.ShowToUser);
			classifications[0].ClassificationReason.Should().Be(ApiDoesNotSupportAllObjectTypes);
		}

		[Test]
		public async Task ClassifyAsync_ShouldProperlyClassifyMultiObjectFields()
		{
			// Arrange 

			var fields = new List<RelativityObject>
			{
				new RelativityObject
				{
					FieldValues = new List<FieldValuePair>
					{
						new FieldValuePair
						{
							Field = new Field
							{
								Name = "Field Type"
							},
							Value = "Multiple Object"
						}
					}
				}
			};

			// Act
			FieldClassificationResult[] classifications = (await _sut.ClassifyAsync(fields, 0).ConfigureAwait(false)).ToArray();

			// Assert
			classifications.Length.Should().Be(1);

			classifications[0].ClassificationLevel.Should().Be(ClassificationLevel.ShowToUser);
			classifications[0].ClassificationReason.Should().Be(ApiDoesNotSupportAllObjectTypes);
		}

		[Test]
		public async Task ClassifyAsync_ShouldProperlyClassifyNonObjectFields()
		{
			// Arrange 

			var fields = new List<RelativityObject>
			{
				new RelativityObject
				{
					FieldValues = new List<FieldValuePair>
					{
						new FieldValuePair
						{
							Field = new Field
							{
								Name = "Field Type"
							},
							Value = "Other type"
						}
					}
				}
			};

			// Act
			FieldClassificationResult[] classifications = (await _sut.ClassifyAsync(fields, 0).ConfigureAwait(false)).ToArray();

			// Assert
			classifications.Length.Should().Be(0);
		}
	}
}
