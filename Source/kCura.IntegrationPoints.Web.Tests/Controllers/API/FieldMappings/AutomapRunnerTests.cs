using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core.TestCategories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Controllers.API.FieldMappings;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Tests.Controllers.API.FieldMappings
{
	[TestFixture, Category("Unit")]
	public class AutomapRunnerTests
	{
		private AutomapRunner _sut;

		[SetUp]
		public void Setup()
		{
			_sut = new AutomapRunner();
		}

		[Test]
		public void MapFields_ShouldMapFieldsWithTheSameTypeAndName()
		{
			// Arrange
			var sourceFields = new[]
			{
				new DocumentFieldInfo
				{
					Name = "Field 1",
					Type = "Type 1"
				},
				new DocumentFieldInfo
				{
					Name = "Field 2",
					Type = "Type 2"
				}
			};

			var destinationFields = new[]
			{
				new DocumentFieldInfo
				{
					Name = "Field 1",
					Type = "Type 1"
				},
				new DocumentFieldInfo
				{
					Name = "Field 2",
					Type = "Type 2"
				}
			};

			// Act
			var mappedFields = _sut.MapFields(sourceFields, destinationFields).ToArray();

			// Assert
			mappedFields.Count().Should().Be(2);

			mappedFields[0].SourceField.DisplayName.Should().Be(mappedFields[0].DestinationField.DisplayName);
			mappedFields[0].SourceField.Type.Should().Be(mappedFields[0].DestinationField.Type);
			mappedFields[0].FieldMapType.Should().Be(FieldMapTypeEnum.None);

			mappedFields[1].SourceField.DisplayName.Should().Be(mappedFields[1].DestinationField.DisplayName);
			mappedFields[1].SourceField.Type.Should().Be(mappedFields[1].DestinationField.Type);
			mappedFields[1].FieldMapType.Should().Be(FieldMapTypeEnum.None);
		}

		[Test]
		public void MapFields_ShouldNotMapFieldsWithTheSameNameButDifferentType()
		{
			// Arrange
			var sourceFields = new[]
			{
				new DocumentFieldInfo
				{
					Name = "Field 1",
					Type = "Type 1"
				},
				new DocumentFieldInfo
				{
					Name = "Field 2",
					Type = "Type 2"
				}
			};

			var destinationFields = new[]
			{
				new DocumentFieldInfo
				{
					Name = "Field 3",
					Type = "Type 1"
				},
				new DocumentFieldInfo
				{
					Name = "Field 2",
					Type = "Type 3"
				}
			};

			// Act
			var mappedFields = _sut.MapFields(sourceFields, destinationFields).ToArray();

			// Assert
			mappedFields.Count().Should().Be(0);
		}

		[Test]
		public void MapFields_ShouldMapFixedLengthTextToLongText()
		{
			// Arrange
			var sourceFields = new[]
			{
				new DocumentFieldInfo
				{
					Name = "Field 1",
					Type = "Fixed-Length Text(250)"
				}
			};

			var destinationFields = new[]
			{
				new DocumentFieldInfo
				{
					Name = "Field 1",
					Type = "Long Text"
				}
			};

			// Act
			var mappedFields = _sut.MapFields(sourceFields, destinationFields).ToArray();

			// Assert
			mappedFields.Count().Should().Be(1);

			mappedFields[0].SourceField.DisplayName.Should().Be(mappedFields[0].DestinationField.DisplayName);
			mappedFields[0].SourceField.Type.Should().Be("Fixed-Length Text(250)");
			mappedFields[0].DestinationField.Type.Should().Be("Long Text");
		}


		[Test]
		public void MapFields_ShouldMapFixedLengthTextToFixedLentghTextWithDifferentSize()
		{
			// Arrange
			var sourceFields = new[]
			{
				new DocumentFieldInfo
				{
					Name = "Field 1",
					Type = "Fixed-Length Text(250)"
				}
			};

			var destinationFields = new[]
			{
				new DocumentFieldInfo
				{
					Name = "Field 1",
					Type = "Fixed-Length Text(50)"
				}
			};

			// Act
			var mappedFields = _sut.MapFields(sourceFields, destinationFields).ToArray();

			// Assert
			mappedFields.Count().Should().Be(1);

			mappedFields[0].SourceField.DisplayName.Should().Be(mappedFields[0].DestinationField.DisplayName);
			mappedFields[0].SourceField.Type.Should().Be("Fixed-Length Text(250)");
			mappedFields[0].DestinationField.Type.Should().Be("Fixed-Length Text(50)");
		}

		[Test]
		public void MapFields_ShouldMapIdentifiersWithDifferentNames()
		{
			// Arrange
			var sourceFields = new[]
			{
				new DocumentFieldInfo
				{
					Name = "Field 1",
					Type = "Fixed-Length Text(250)",
					IsIdentifier = true
				}
			};

			var destinationFields = new[]
			{
				new DocumentFieldInfo
				{
					Name = "Field 1",
					Type = "Fixed-Length Text(50)",
					IsIdentifier = false
				},
				new DocumentFieldInfo
				{
					Name = "Field 2",
					Type = "Fixed-Length Text(50)",
					IsIdentifier = true
				}
			};

			// Act
			var mappedFields = _sut.MapFields(sourceFields, destinationFields).ToArray();

			// Assert
			mappedFields.Count().Should().Be(1);
			mappedFields[0].SourceField.DisplayName.Should().Be("Field 1");
			mappedFields[0].DestinationField.DisplayName.Should().Be("Field 2");
			mappedFields[0].FieldMapType.Should().Be(FieldMapTypeEnum.Identifier);
		}

		[Test]
		public void MapFields_ShouldMapOnlyIdentifiers_When_ParameterIsSet()
		{
			// Arrange
			var sourceFields = new[]
			{
				new DocumentFieldInfo
				{
					Name = "Field 1",
					Type = "Fixed-Length Text(250)",
					IsIdentifier = true
				},
				new DocumentFieldInfo
				{
					Name = "Field 2",
					Type = "Fixed-Length Text(250)",
				}
			};

			var destinationFields = new[]
			{
				new DocumentFieldInfo
				{
					Name = "Field 1",
					Type = "Fixed-Length Text(50)",
					IsIdentifier = true
				},
				new DocumentFieldInfo
				{
					Name = "Field 2",
					Type = "Fixed-Length Text(50)",
				}
			};

			// Act
			var mappedFields = _sut.MapFields(sourceFields, destinationFields, true).ToArray();

			// Assert
			mappedFields.Count().Should().Be(1);
			mappedFields[0].SourceField.DisplayName.Should().Be("Field 1");
			mappedFields[0].DestinationField.DisplayName.Should().Be("Field 1");
			mappedFields[0].FieldMapType.Should().Be(FieldMapTypeEnum.Identifier);
		}

		[Test]
		public void MapFields_ShouldReturnMappingsInAlphabeticalOrder()
		{
			// Arrange
			var sourceFields = new[]
			{
				new DocumentFieldInfo
				{
					Name = "Field 1",
					Type = "Fixed-Length Text(250)"
				},
				new DocumentFieldInfo
				{
					Name = "Field 3",
					Type = "Fixed-Length Text(250)"
				},new DocumentFieldInfo
				{
					Name = "Field 2",
					Type = "Fixed-Length Text(250)"
				},

			};

			var destinationFields = new[]
			{
				new DocumentFieldInfo
				{
					Name = "Field 3",
					Type = "Fixed-Length Text(250)"
				},
				new DocumentFieldInfo
				{
					Name = "Field 2",
					Type = "Fixed-Length Text(250)"
				},new DocumentFieldInfo
				{
					Name = "Field 1",
					Type = "Fixed-Length Text(250)"
				},
			};

			// Act
			var mappedFields = _sut.MapFields(sourceFields, destinationFields).ToArray();

			// Assert
			mappedFields.Count().Should().Be(3);
			mappedFields[0].SourceField.DisplayName.Should().Be("Field 1");
			mappedFields[1].SourceField.DisplayName.Should().Be("Field 2");
			mappedFields[2].SourceField.DisplayName.Should().Be("Field 3");
		
		}
	}
}
