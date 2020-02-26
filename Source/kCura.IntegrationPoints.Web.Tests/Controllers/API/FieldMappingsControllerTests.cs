using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using FluentAssertions;
using kCura.IntegrationPoints.DocumentTransferProvider;
using kCura.IntegrationPoints.Web.Controllers.API.FieldMappings;
using Moq;
using NUnit.Framework;
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

		[SetUp]
		public void SetUp()
		{
			_automapRunnerMock = new Mock<IAutomapRunner>();
			_fieldsMappingValidator = new Mock<IFieldsMappingValidator>();

			_fieldsClassifierRunner = new Mock<IFieldsClassifierRunner>();
			_fieldsClassifierRunner.Setup(x => x.GetFilteredFieldsAsync(It.IsAny<int>())).ReturnsAsync(new List<FieldClassificationResult>());

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
			// Arrange
			const int workspaceId = 123456;

			// Act
			HttpResponseMessage responseMessage = await _sut.GetMappableFieldsFromSourceWorkspace(workspaceId).ConfigureAwait(false);

			// Assert
			responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

			_fieldsClassifierRunner
				.Verify(x => x.GetFilteredFieldsAsync(workspaceId),
					Times.Once);
		}

		[Test]
		public async Task GetMappableFieldsFromDestinationWorkspace_ShouldFilterFields()
		{
			// Arrange
			const int workspaceId = 123456;

			// Act
			HttpResponseMessage responseMessage = await _sut.GetMappableFieldsFromDestinationWorkspace(workspaceId).ConfigureAwait(false);

			// Assert
			responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

			_fieldsClassifierRunner
				.Verify(x => x.GetFilteredFieldsAsync(workspaceId),
					Times.Once);
		}

		[Test]
		public async Task ValidateAsync_ShouldValidateFieldsMap()
		{
			// Arrange
			int sourceWorkspaceID = 1;
			int destinationWorkspaceID = 2;
			IEnumerable<FieldMap> fieldMap = new List<FieldMap>();

			// Act
			HttpResponseMessage responseMessage = await _sut.ValidateAsync(fieldMap, sourceWorkspaceID, destinationWorkspaceID).ConfigureAwait(false);

			// Assert
			responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

			_fieldsMappingValidator
				.Verify(x => x.ValidateAsync(fieldMap, sourceWorkspaceID, destinationWorkspaceID),
					Times.Once);
		}
	}
}