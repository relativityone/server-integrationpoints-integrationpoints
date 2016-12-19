using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Hosting;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Controllers.API;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Tests.Controllers.API
{
	[TestFixture]
	public class ProductionControllerTests : TestBase
	{
		private ProductionController _controller;
		private IProductionService _service;

		[SetUp]
		public override void SetUp()
		{
			_service = Substitute.For<IProductionService>();
			_controller = new ProductionController(_service);
			_controller.Request = new HttpRequestMessage();
			_controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
		}

		[Test]
		public void ItShouldReturnSortedProductions()
		{
			var production1 = new ProductionDTO
			{
				ArtifactID = "1",
				DisplayName = "A"
			};
			var production2 = new ProductionDTO
			{
				ArtifactID = "2",
				DisplayName = "B"
			};
			var expectedResult = new List<ProductionDTO> {production1, production2};

			var productions = new List<ProductionDTO> {production2, production1};
			_service.GetProductionsForExport(0).ReturnsForAnyArgs(productions);

			var responseMessage = _controller.GetProductionsForExport(0);
			var actualResult = ExtractResponse(responseMessage);

			CollectionAssert.AreEqual(expectedResult, actualResult);
		}

		[Test]
		public void ItShouldReturnImportProductions()
		{
			var production1 = new ProductionDTO
			{
				ArtifactID = "1",
				DisplayName = "A"
			};
			var production2 = new ProductionDTO
			{
				ArtifactID = "2",
				DisplayName = "B"
			};

			var production3 = new ProductionDTO
			{
				ArtifactID = "0",
				DisplayName = string.Empty
			};

			//It will sort alphabetically and filter out the empty production
			var expectedResult = new List<ProductionDTO> { production1, production2 };

			var productions = new List<ProductionDTO> { production2, production1, production3 };
			_service.GetProductionsForImport(0).ReturnsForAnyArgs(productions);

			var responseMessage = _controller.GetProductionsForImport(0);
			var actualResult = ExtractResponse(responseMessage);

			CollectionAssert.AreEqual(expectedResult, actualResult);
		}

		[Test]
		public void ItShouldSortProductionsIgnoringCase()
		{
			var production1 = new ProductionDTO
			{
				ArtifactID = "1",
				DisplayName = "abc"
			};
			var production2 = new ProductionDTO
			{
				ArtifactID = "2",
				DisplayName = "Bcd"
			};
			var expectedResult = new List<ProductionDTO> { production1, production2 };

			var productions = new List<ProductionDTO> { production2, production1 };
			_service.GetProductionsForExport(0).ReturnsForAnyArgs(productions);

			var responseMessage = _controller.GetProductionsForExport(0);
			var actualResult = ExtractResponse(responseMessage);

			CollectionAssert.AreEqual(expectedResult, actualResult);
		}

		private IEnumerable<ProductionDTO> ExtractResponse(HttpResponseMessage response)
		{
			var objectContent = response.Content as ObjectContent;
			var list = (IEnumerable<ProductionDTO>) objectContent?.Value;

			return list;
		}
	}
}