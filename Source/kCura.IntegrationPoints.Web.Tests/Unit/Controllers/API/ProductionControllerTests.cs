﻿using System.Collections.Generic;
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
	public class ProductionControllerTests
	{
		private ProductionController _controller;
		private IProductionService _service;

		[SetUp]
		public void SetUp()
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
			_service.GetProductions(0).ReturnsForAnyArgs(productions);

			var responseMessage = _controller.GetProductions(0);
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
			_service.GetProductions(0).ReturnsForAnyArgs(productions);

			var responseMessage = _controller.GetProductions(0);
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