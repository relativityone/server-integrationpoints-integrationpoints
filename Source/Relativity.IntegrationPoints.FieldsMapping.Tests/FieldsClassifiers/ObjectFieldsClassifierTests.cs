using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.IntegrationPoints.FieldsMapping.FieldClassifiers;

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

			var fields = new List<FieldInfo>
			{
				new FieldInfo(fieldIdentifier: "1", name: "Field 1", type: "Single Object")
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

			var fields = new List<FieldInfo>
			{
				new FieldInfo(fieldIdentifier: "1", name: "Field 1", type: "Multiple Object")
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

			var fields = new List<FieldInfo>
			{
				new FieldInfo(fieldIdentifier: "1", name: "Field 1", type: "Other type")
			};

			// Act
			FieldClassificationResult[] classifications = (await _sut.ClassifyAsync(fields, 0).ConfigureAwait(false)).ToArray();

			// Assert
			classifications.Length.Should().Be(0);
		}
	}
}
