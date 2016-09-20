using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Hosting;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Controllers.API;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Tests.Unit.Controllers.API
{
	public class ProductionPrecedenceControllerTests
	{
		private ProductionPrecedenceController _controller;
		private IProductionPrecedenceService _service;

		[SetUp]
		public void SetUp()
		{
			_service = Substitute.For<IProductionPrecedenceService>();
			_controller = new ProductionPrecedenceController(_service);
			_controller.Request = new HttpRequestMessage();
			_controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
		}

		[Test]
		public void ItShouldReturnSortedProductions()
		{
			var production1 = new ProductionPrecedenceDTO
			{
				ArtifactID = "1",
				DisplayName = "A"
			};
			var production2 = new ProductionPrecedenceDTO
			{
				ArtifactID = "2",
				DisplayName = "B"
			};
			var expectedResult = new List<ProductionPrecedenceDTO> {production1, production2};

			var productions = new List<ProductionPrecedenceDTO> {production2, production1};
			_service.GetProductionPrecedence(0).ReturnsForAnyArgs(productions);

			var responseMessage = _controller.GetProductionPrecedence(0);
			var actualResult = ExtractResponse(responseMessage);

			CollectionAssert.AreEqual(expectedResult, actualResult);
		}

		[Test]
		public void ItShouldSortProductionsIgnoringCase()
		{
			var production1 = new ProductionPrecedenceDTO
			{
				ArtifactID = "1",
				DisplayName = "abc"
			};
			var production2 = new ProductionPrecedenceDTO
			{
				ArtifactID = "2",
				DisplayName = "Bcd"
			};
			var expectedResult = new List<ProductionPrecedenceDTO> { production1, production2 };

			var productions = new List<ProductionPrecedenceDTO> { production2, production1 };
			_service.GetProductionPrecedence(0).ReturnsForAnyArgs(productions);

			var responseMessage = _controller.GetProductionPrecedence(0);
			var actualResult = ExtractResponse(responseMessage);

			CollectionAssert.AreEqual(expectedResult, actualResult);
		}

		private IEnumerable<ProductionPrecedenceDTO> ExtractResponse(HttpResponseMessage response)
		{
			var objectContent = response.Content as ObjectContent;
			var list = (IEnumerable<ProductionPrecedenceDTO>) objectContent?.Value;

			return list;
		}
	}
}