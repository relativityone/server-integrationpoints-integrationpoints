using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Domain.Models;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Field;
using Relativity.Services.Search;
using CategoryAttribute = NUnit.Framework.CategoryAttribute;

namespace Relativity.IntegrationPoints.FieldsMapping.Tests
{
	[TestFixture, Category("Unit")]
	public class AutomapRunnerTests
	{
		private Mock<IKeywordSearchManager> _keywordSearchManagerFake;
		private Mock<IServicesMgr> _servicesMgrFake;
		private AutomapRunner _sut;

		[SetUp]
		public void Setup()
		{
			_keywordSearchManagerFake = new Mock<IKeywordSearchManager>();
			_servicesMgrFake = new Mock<IServicesMgr>();
			_servicesMgrFake.Setup(x => x.CreateProxy<IKeywordSearchManager>(It.IsAny<ExecutionIdentity>()))
				.Returns(_keywordSearchManagerFake.Object);
			_sut = new AutomapRunner(_servicesMgrFake.Object);
		}

		[Test]
		public void MapFields_ShouldMapFieldsWithTheSameTypeAndName()
		{
			// Arrange
			var sourceFields = new[]
			{
				new DocumentFieldInfo
				{
					FieldIdentifier = "1",
					Name = "Field 1",
					Type = "Type 1",
				},
				new DocumentFieldInfo
				{
					FieldIdentifier = "2", 
					Name = "Field 2",
					Type = "Type 2"
				}
			};

			var destinationFields = new[]
			{
				new DocumentFieldInfo
				{
					FieldIdentifier = "3", 
					Name = "Field 1",
					Type = "Type 1"
				},
				new DocumentFieldInfo
				{
					FieldIdentifier = "4", 
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
		public void MapFields_ShouldMapFieldsWithTheSameIdentifierAndDifferentName()
		{
			// Arrange
			var sourceFields = new[]
			{
				new DocumentFieldInfo
				{
					FieldIdentifier = "1",
					Name = "Field 1",
					Type = "Type 1",
				},
				new DocumentFieldInfo
				{
					FieldIdentifier = "2",
					Name = "Field 2",
					Type = "Type 2"
				}
			};

			var destinationFields = new[]
			{
				new DocumentFieldInfo
				{
					FieldIdentifier = "1",
					Name = "Field 3",
					Type = "Type 1"
				},
				new DocumentFieldInfo
				{
					FieldIdentifier = "2",
					Name = "Field 4",
					Type = "Type 2"
				}
			};

			// Act
			var mappedFields = _sut.MapFields(sourceFields, destinationFields).ToArray();

			// Assert
			mappedFields.Count().Should().Be(2);

			mappedFields[0].SourceField.FieldIdentifier.Should().Be(mappedFields[0].DestinationField.FieldIdentifier);
			mappedFields[0].SourceField.Type.Should().Be(mappedFields[0].DestinationField.Type);
			mappedFields[0].FieldMapType.Should().Be(FieldMapTypeEnum.None);

			mappedFields[1].SourceField.FieldIdentifier.Should().Be(mappedFields[1].DestinationField.FieldIdentifier);
			mappedFields[1].SourceField.Type.Should().Be(mappedFields[1].DestinationField.Type);
			mappedFields[1].FieldMapType.Should().Be(FieldMapTypeEnum.None);
		}

		[Test]
		public void MapFields_ShouldMapFieldsWithTheSameIdentifierBeforeName()
		{
			// Arrange
			var sourceFields = new[]
			{
				new DocumentFieldInfo
				{
					FieldIdentifier = "1",
					Name = "Field 1",
					Type = "Type 1",
				},
				new DocumentFieldInfo
				{
					FieldIdentifier = "2",
					Name = "Field 2",
					Type = "Type 2"
				}
			};

			var destinationFields = new[]
			{
				new DocumentFieldInfo
				{
					FieldIdentifier = "1",
					Name = "Field 2",
					Type = "Type 1"
				},
				new DocumentFieldInfo
				{
					FieldIdentifier = "2",
					Name = "Field 1",
					Type = "Type 2"
				}
			};

			// Act
			var mappedFields = _sut.MapFields(sourceFields, destinationFields).ToArray();

			// Assert
			mappedFields.Count().Should().Be(2);

			mappedFields[0].SourceField.FieldIdentifier.Should().Be(mappedFields[0].DestinationField.FieldIdentifier);
			mappedFields[0].SourceField.Type.Should().Be(mappedFields[0].DestinationField.Type);
			mappedFields[0].FieldMapType.Should().Be(FieldMapTypeEnum.None);

			mappedFields[1].SourceField.FieldIdentifier.Should().Be(mappedFields[1].DestinationField.FieldIdentifier);
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
					FieldIdentifier = "1", 
					Name = "Field 1",
					Type = "Type 1"
				},
				new DocumentFieldInfo
				{
					FieldIdentifier = "2", 
					Name = "Field 2",
					Type = "Type 2"
				}
			};

			var destinationFields = new[]
			{
				new DocumentFieldInfo
				{
					FieldIdentifier = "3", 
					Name = "Field 3",
					Type = "Type 1"
				},
				new DocumentFieldInfo
				{
					FieldIdentifier = "4", 
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
					FieldIdentifier = "1", 
					Name = "Field 1",
					Type = "Fixed-Length Text(250)"
				}
			};

			var destinationFields = new[]
			{
				new DocumentFieldInfo
				{
					FieldIdentifier = "2", 
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
					FieldIdentifier = "1", 
					Name = "Field 1",
					Type = "Fixed-Length Text(250)"
				}
			};

			var destinationFields = new[]
			{
				new DocumentFieldInfo
				{
					FieldIdentifier = "2", 
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
					FieldIdentifier = "1", 
					Name = "Field 1",
					Type = "Fixed-Length Text(250)",
					IsIdentifier = true
				}
			};

			var destinationFields = new[]
			{
				new DocumentFieldInfo
				{
					FieldIdentifier = "2", 
					Name = "Field 1",
					Type = "Fixed-Length Text(50)",
					IsIdentifier = false
				},
				new DocumentFieldInfo
				{
					FieldIdentifier = "3", 
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
					FieldIdentifier = "1", 
					Name = "Field 1",
					Type = "Fixed-Length Text(250)",
					IsIdentifier = true
				},
				new DocumentFieldInfo
				{
					FieldIdentifier = "2", 
					Name = "Field 2",
					Type = "Fixed-Length Text(250)",
				}
			};

			var destinationFields = new[]
			{
				new DocumentFieldInfo
				{
					FieldIdentifier = "3", 
					Name = "Field 1",
					Type = "Fixed-Length Text(50)",
					IsIdentifier = true
				},
				new DocumentFieldInfo
				{
					FieldIdentifier = "4", 
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
					FieldIdentifier = "1", 
					Name = "Field 1",
					Type = "Fixed-Length Text(250)"
				},
				new DocumentFieldInfo
				{
					FieldIdentifier = "2", 
					Name = "Field 3",
					Type = "Fixed-Length Text(250)"
				},new DocumentFieldInfo
				{
					FieldIdentifier = "3", 
					Name = "Field 2",
					Type = "Fixed-Length Text(250)"
				},

			};

			var destinationFields = new[]
			{
				new DocumentFieldInfo
				{
					FieldIdentifier = "4", 
					Name = "Field 3",
					Type = "Fixed-Length Text(250)"
				},
				new DocumentFieldInfo
				{
					FieldIdentifier = "5", 
					Name = "Field 2",
					Type = "Fixed-Length Text(250)"
				},new DocumentFieldInfo
				{
					FieldIdentifier = "6", 
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

		[Test]
		public async Task MapFieldsFromSavedSearch_ShouldMapFieldFromSavedSearch()
		{
			// Arrange

			var savedSearchFields = new List<FieldRef>
			{
				new FieldRef()
				{
					ArtifactID = 2,
					Name = "Field 2"
				}
			};

			var sourceFields = new[]
			{
				new DocumentFieldInfo
				{
					FieldIdentifier = "1",
					Name = "Field 1",
					Type = "Fixed-Length Text(250)"
				},
				new DocumentFieldInfo
				{
					FieldIdentifier = "2",
					Name = "Field 2",
					Type = "Fixed-Length Text(250)"
				}
			};

			var destinationFields = new[]
			{
				new DocumentFieldInfo
				{
					FieldIdentifier = "3",
					Name = "Field 3",
					Type = "Fixed-Length Text(250)"
				},
				new DocumentFieldInfo
				{
					FieldIdentifier = "4",
					Name = "Field 2",
					Type = "Fixed-Length Text(250)"
				}
			};
			
			_keywordSearchManagerFake.Setup(x => x.ReadSingleAsync(It.IsAny<int>(), It.IsAny<int>()))
				.ReturnsAsync(new KeywordSearch()
				{
					Fields = savedSearchFields
				});

			// Act
			var mappedFields = (await _sut.MapFieldsFromSavedSearchAsync(sourceFields, destinationFields, 1, 2)
				.ConfigureAwait(false)).ToArray();

			// Assert
			mappedFields.Length.Should().Be(1);
			mappedFields.Single().SourceField.DisplayName.Should().Be("Field 2");
		}

		[Test]
		public async Task MapFieldsFromSavedSearch_ShouldMapAlsoObjectIdentifier()
		{
			// Arrange

			var savedSearchFields = new List<FieldRef>
			{
				new FieldRef()
				{
					ArtifactID = 2,
					Name = "Field 2"
				}
			};

			var sourceFields = new[]
			{
				new DocumentFieldInfo
				{
					FieldIdentifier = "1",
					Name = "ControlNumber",
					Type = "Fixed-Length Text(250)",
					IsIdentifier = true
				},
				new DocumentFieldInfo
				{
					FieldIdentifier = "2",
					Name = "Field 2",
					Type = "Fixed-Length Text(250)"
				}
			};

			var destinationFields = new[]
			{  
				new DocumentFieldInfo
				{
					FieldIdentifier = "10",
					Name = "ControlNumber",
					Type = "Fixed-Length Text(250)",
					IsIdentifier = true
				},
				new DocumentFieldInfo
				{
					FieldIdentifier = "3",
					Name = "Field 3",
					Type = "Fixed-Length Text(250)"
				},
				new DocumentFieldInfo
				{
					FieldIdentifier = "4",
					Name = "Field 2",
					Type = "Fixed-Length Text(250)"
				},
				new DocumentFieldInfo
				{
					FieldIdentifier = "5",
					Name = "Field 5",
					Type = "Fixed-Length Text(250)"
				}
			};

			_keywordSearchManagerFake.Setup(x => x.ReadSingleAsync(It.IsAny<int>(), It.IsAny<int>()))
				.ReturnsAsync(new KeywordSearch()
				{
					Fields = savedSearchFields
				});

			// Act
			var mappedFields = (await _sut.MapFieldsFromSavedSearchAsync(sourceFields, destinationFields, 1, 2)
				.ConfigureAwait(false)).ToArray();

			// Assert
			mappedFields.Length.Should().Be(2);
			mappedFields[0].SourceField.DisplayName.Should().Be("ControlNumber");
			mappedFields[1].SourceField.DisplayName.Should().Be("Field 2");
		}
		
	}
}
