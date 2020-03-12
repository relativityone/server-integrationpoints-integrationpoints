﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using FluentAssertions;
using kCura.IntegrationPoints.DocumentTransferProvider;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Controllers.API.FieldMappings;
using kCura.IntegrationPoints.Web.Models;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping;
using Relativity.IntegrationPoints.FieldsMapping.FieldClassifiers;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Web.Tests.Controllers.API.FieldMappings
{
	[TestFixture, Category("Unit")]
	public class FieldMappingsControllerTests
	{
		private FieldMappingsController _sut;

		private Mock<IFieldsClassifyRunnerFactory> _fieldsClassifyRunnerFactoryMock;
		private Mock<IFieldsClassifierRunner> _fieldsClassifierRunner;
		private Mock<IAutomapRunner> _automapRunnerMock;
		private Mock<IFieldsMappingValidator> _fieldsMappingValidator;
		private List<FieldClassificationResult> _sourceFieldClassificationResults;
		private List<FieldClassificationResult> _destinationFieldClassificationResults;
		private IEnumerable<ClassifiedFieldDTO> _sourceClassifiedFieldDTOs;
		private IEnumerable<ClassifiedFieldDTO> _destinationClassifiedFieldDTOs;

		private const int _SOURCE_WORKSPACE_ID = 1;
		private const int _DESTINATION_WORKSPACE_ID = 2;

		[SetUp]
		public void SetUp()
		{
			_automapRunnerMock = new Mock<IAutomapRunner>();
			_fieldsMappingValidator = new Mock<IFieldsMappingValidator>();

			_fieldsClassifierRunner = new Mock<IFieldsClassifierRunner>();
			_sourceFieldClassificationResults = new List<FieldClassificationResult> { new FieldClassificationResult(new DocumentFieldInfo("1", "Name", "Type")) };
			_destinationFieldClassificationResults = new List<FieldClassificationResult> { new FieldClassificationResult(new DocumentFieldInfo("2", "Name", "Type")) };

			_sourceClassifiedFieldDTOs = _sourceFieldClassificationResults.Select(x => new ClassifiedFieldDTO(x));
			_destinationClassifiedFieldDTOs = _destinationFieldClassificationResults.Select(x => new ClassifiedFieldDTO(x));

			_fieldsClassifierRunner.Setup(x => x.GetFilteredFieldsAsync(_SOURCE_WORKSPACE_ID)).ReturnsAsync(_sourceFieldClassificationResults);
			_fieldsClassifierRunner.Setup(x => x.GetFilteredFieldsAsync(_DESTINATION_WORKSPACE_ID)).ReturnsAsync(_destinationFieldClassificationResults);

			_fieldsClassifyRunnerFactoryMock = new Mock<IFieldsClassifyRunnerFactory>();
			_fieldsClassifyRunnerFactoryMock.Setup(m => m.CreateForSourceWorkspace()).Returns(_fieldsClassifierRunner.Object);
			_fieldsClassifyRunnerFactoryMock.Setup(m => m.CreateForDestinationWorkspace()).Returns(_fieldsClassifierRunner.Object);

			_sut = new FieldMappingsController(_fieldsClassifyRunnerFactoryMock.Object, _automapRunnerMock.Object, _fieldsMappingValidator.Object)
			{
				Configuration = new HttpConfiguration(),
				Request = new HttpRequestMessage()
			};
		}

		[Test]
		public async Task GetMappableFieldsFromSourceWorkspace_ShouldFilterFields()
		{
			// Act
			HttpResponseMessage responseMessage = await _sut.GetMappableFieldsFromSourceWorkspace(_SOURCE_WORKSPACE_ID).ConfigureAwait(false);
			string jsonResponse = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

			// Assert
			responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

			_fieldsClassifierRunner
				.Verify(x => x.GetFilteredFieldsAsync(_SOURCE_WORKSPACE_ID),
					Times.Once);

			jsonResponse.Should().Be(JsonConvert.SerializeObject(_sourceClassifiedFieldDTOs));
		}

		[Test]
		public async Task GetMappableFieldsFromDestinationWorkspace_ShouldFilterFields()
		{
			// Act
			HttpResponseMessage responseMessage = await _sut.GetMappableFieldsFromDestinationWorkspace(_DESTINATION_WORKSPACE_ID).ConfigureAwait(false);
			string jsonResponse = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

			// Assert
			responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

			_fieldsClassifierRunner
				.Verify(x => x.GetFilteredFieldsAsync(_DESTINATION_WORKSPACE_ID),
					Times.Once);

			jsonResponse.Should().Be(JsonConvert.SerializeObject(_destinationClassifiedFieldDTOs));
		}

		[Test]
		public async Task ValidateAsync_ShouldValidateFieldsMap()
		{
			// Arrange
			IEnumerable<FieldMap> fieldMap = new List<FieldMap>();
			IEnumerable<FieldMap> validationResult = new List<FieldMap>
			{
				new FieldMap
				{
					SourceField = new FieldEntry
					{
						FieldIdentifier = "1"
					},
					DestinationField = new FieldEntry
					{
						FieldIdentifier = "2"
					},
					FieldMapType = FieldMapTypeEnum.None
				}
			};

			_fieldsMappingValidator.Setup(x => x.ValidateAsync(fieldMap, _SOURCE_WORKSPACE_ID, _DESTINATION_WORKSPACE_ID)).ReturnsAsync(validationResult);

			// Act
			HttpResponseMessage responseMessage = await _sut.ValidateAsync(fieldMap, _SOURCE_WORKSPACE_ID, _DESTINATION_WORKSPACE_ID).ConfigureAwait(false);
			string jsonResponse = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

			// Assert
			responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

			_fieldsMappingValidator
				.Verify(x => x.ValidateAsync(fieldMap, _SOURCE_WORKSPACE_ID, _DESTINATION_WORKSPACE_ID),
					Times.Once);

			jsonResponse.Should().Be(JsonConvert.SerializeObject(validationResult, _sut.Configuration.Formatters.JsonFormatter.SerializerSettings));
		}
	}
}