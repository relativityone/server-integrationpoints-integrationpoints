using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Hosting;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Web.Controllers.API;
using kCura.IntegrationPoints.Web.Models;
using NSubstitute;
using NUnit.Framework;
using FluentAssertions;
using NSubstitute.ExceptionExtensions;

// ReSharper disable AssignNullToNotNullAttribute

namespace kCura.IntegrationPoints.Web.Tests.Controllers
{
    [TestFixture, Category("Unit")]
    public class IntegrationPointTypesControllerTests
    {
        [Test]
        public void ReturnModelObjectsInRightOrder()
        {
            // ARRANGE
            var integrationPointTypeService = Substitute.For<IIntegrationPointTypeService>();
            integrationPointTypeService.GetAllIntegrationPointTypes()
                .Returns(
                    new List<IntegrationPointType>
                    {
                        new IntegrationPointType
                        {
                            Name = "Name 1",
                            ArtifactId = 303,
                            Identifier = "Identifier 1"
                        },
                        new IntegrationPointType
                        {
                            Name = "Name 2",
                            ArtifactId = 303,
                            Identifier = "Identifier 2"
                        }
                    });
            IntegrationPointTypesController controller = CreateController(integrationPointTypeService);

            // ACT
            HttpResponseMessage response = controller.Get();

            // ASSERT
            var objectContent = response.Content as ObjectContent;
            List<IntegrationPointTypeModel> modelsList = ((IEnumerable<IntegrationPointTypeModel>) objectContent?.Value)
                .ToList();

            modelsList.Should().HaveCount(2);
            modelsList.First().Name.Should().Be("Name 1");
            modelsList.Last().Name.Should().Be("Name 2");
        }

        [Test]
        public void ReturnEmptyCollectionIfNoTypesCanBeFound()
        {
            // ARRANGE
            var integrationPointTypeService = Substitute.For<IIntegrationPointTypeService>();
            integrationPointTypeService.GetAllIntegrationPointTypes().Returns(new List<IntegrationPointType>());

            IntegrationPointTypesController controller = CreateController(integrationPointTypeService);

            // ACT
            HttpResponseMessage response = controller.Get();

            // ASSERT
            var objectContent = response.Content as ObjectContent;
            List<IntegrationPointTypeModel> modelsList = ((IEnumerable<IntegrationPointTypeModel>) objectContent?.Value)
                .ToList();

            modelsList.Should().BeEmpty();
        }

        [Test]
        public void ThrowExceptionIfSeviceReturnsNull()
        {
            // ARRANGE
            var integrationPointTypeService = Substitute.For<IIntegrationPointTypeService>();
            integrationPointTypeService.GetAllIntegrationPointTypes().Returns((List<IntegrationPointType>) null);

            IntegrationPointTypesController controller = CreateController(integrationPointTypeService);

            // ACT
            Action act = () => controller.Get();

            // ASSERT
            act.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void PropagateExceptionThrownByService()
        {
            // ARRANGE
            var integrationPointTypeService = Substitute.For<IIntegrationPointTypeService>();
            integrationPointTypeService.GetAllIntegrationPointTypes().ThrowsForAnyArgs(new Exception());

            IntegrationPointTypesController controller = CreateController(integrationPointTypeService);

            // ACT
            Action act = () => controller.Get();

            // ASSERT
            act.ShouldThrow<Exception>();
        }

        private static IntegrationPointTypesController CreateController(IIntegrationPointTypeService service)
        {
            var controller = new IntegrationPointTypesController(service) {Request = new HttpRequestMessage()};
            controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            return controller;
        }
    }
}
