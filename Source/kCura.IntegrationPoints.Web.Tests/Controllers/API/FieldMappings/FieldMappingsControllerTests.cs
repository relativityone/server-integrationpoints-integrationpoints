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
using kCura.IntegrationPoints.Web.Controllers.API.FieldMappings.FieldClassifiers;
using Moq;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Tests.Controllers.API.FieldMappings
{
	[TestFixture]
	public class FieldMappingsControllerTests
	{
		private FieldMappingsController _sut;
		private Mock<IFieldsClassifierRunner> _hideFromUserFieldsFilterMock;

		[SetUp]
		public void SetUp()
		{
			_hideFromUserFieldsFilterMock = new Mock<IFieldsClassifierRunner>();
			_hideFromUserFieldsFilterMock
				.Setup(x => x.GetFilteredFieldsAsync(It.IsAny<int>(), It.IsAny<IList<IFieldsClassifier>>()))
				.ReturnsAsync(new List<FieldClassificationResult>());

			var importApiFactoryStub = new Mock<IImportApiFactory>();

			_sut = new FieldMappingsController(_hideFromUserFieldsFilterMock.Object, importApiFactoryStub.Object)
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
			IList<Type> types = new List<Type>()
			{
				typeof(RipFieldsClassifier),
				typeof(SystemFieldsClassifier),
				typeof(NotSupportedByIAPIFieldsClassifier)
			};

			// Act
			HttpResponseMessage responseMessage = await _sut.GetMappableFieldsFromSourceWorkspace(workspaceId).ConfigureAwait(false);

			// Assert
			responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

			_hideFromUserFieldsFilterMock
				.Verify(x => x.GetFilteredFieldsAsync(It.Is<int>(y => y == workspaceId), It.Is<IList<IFieldsClassifier>>(y => ShouldContainObjectOfTypes(y, types))),
					Times.Once);
		}

		[Test]
		public async Task GetMappableFieldsFromDestinationWorkspace_ShouldFilterFields()
		{
			// Arrange
			const int workspaceId = 123456;
			IList<Type> types = new List<Type>()
			{
				typeof(RipFieldsClassifier),
				typeof(SystemFieldsClassifier),
				typeof(NotSupportedByIAPIFieldsClassifier),
				typeof(OpenToAssociationsFieldsClassifier)
			};

			// Act
			HttpResponseMessage responseMessage = await _sut.GetMappableFieldsFromDestinationWorkspace(workspaceId).ConfigureAwait(false);

			// Assert
			responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

			_hideFromUserFieldsFilterMock
				.Verify(x => x.GetFilteredFieldsAsync(It.Is<int>(y => y == workspaceId), It.Is<IList<IFieldsClassifier>>(y => ShouldContainObjectOfTypes(y, types))),
					Times.Once);
		}

		private static bool ShouldContainObjectOfTypes(IList<IFieldsClassifier> list, IList<Type> types)
		{
			return types.ForAll(type => list.Any(type.IsInstanceOfType));
		}
	}
}