using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services.DestinationTypes;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Web.Controllers.API;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Tests.Controllers
{
    [TestFixture, Category("Unit")]
    class DestinationTypeControllerTests : TestBase
    {
        private DestinationTypeController _instance;
        private IWindsorContainer _windsorContainer;
        private IDestinationTypeFactory _destinationTypeFactory;
        private ICaseServiceContext _iCaseServiceContext;
        private IObjectTypeRepository _objTypeQuery;
        private Guid _documentObjectGuid;
        private Guid _randomRdoGuid;

        private void SetUpWindsorContainer()
        {
            _windsorContainer.Register(Component.For<IDestinationTypeFactory>().Instance(_destinationTypeFactory).LifestyleTransient());
            _windsorContainer.Register(Component.For<DestinationTypeController>());
            _windsorContainer.Register(Component.For<ICaseServiceContext>().Instance(_iCaseServiceContext).LifestyleTransient());
            _windsorContainer.Register(Component.For<IObjectTypeRepository>().Instance(_objTypeQuery).LifestyleTransient());
        }

        [SetUp]
        public override void SetUp()
        {
            _windsorContainer = new WindsorContainer();
            _destinationTypeFactory = Substitute.For<IDestinationTypeFactory>();
            _iCaseServiceContext = Substitute.For<ICaseServiceContext>();
            _iCaseServiceContext.WorkspaceUserID.Returns(-1);

            _documentObjectGuid = new Guid("15C36703-74EA-4FF8-9DFB-AD30ECE7530D");
            _randomRdoGuid = new Guid("b73de172-aa9c-4f9a-bd1a-947112804f82");
            Dictionary<Guid, int> guidToTypeId = new Dictionary<Guid, int>()
            {
                {_documentObjectGuid, 10},
                {_randomRdoGuid, 789456 }
            };
            _objTypeQuery = Substitute.For<IObjectTypeRepository>();
            _objTypeQuery.GetRdoGuidToArtifactIdMap().Returns(guidToTypeId);

            var config = new HttpConfiguration();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/Get");
            var route = config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{id}");
            var routeData = new HttpRouteData(route, new HttpRouteValueDictionary { { "controller", "SourceTypeController" } });

            this.SetUpWindsorContainer();

            // Set up Request on controller
            _instance = _windsorContainer.Kernel.Resolve<DestinationTypeController>();
            _instance.ControllerContext = new HttpControllerContext(config, routeData, request);
            _instance.Request = request;
            _instance.Request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;

        }

        [Test]
        public void Get_GoldFlow()
        {
            // Arrange
            IEnumerable<DestinationType> destinationTypeModels = new List<DestinationType>()
            {
                new DestinationType()
                {
                    Name = "name",
                    ID = "d39d9a5e-e009-4c33-b112-73cc45c2ae2d", // some random guid
                    ArtifactID = 123,
                },
                new DestinationType()
                {
                    Name = "name",
                    ID = "423b4d43-eae9-4e14-b767-17d629de4bb2",
                    ArtifactID = 123,

                }
            };


            _destinationTypeFactory.GetDestinationTypes().Returns(destinationTypeModels);

            // Act
            HttpResponseMessage response = _instance.Get();

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            StringAssert.AreEqualIgnoringCase(
                "[{\"Name\":\"name\",\"ID\":\"d39d9a5e-e009-4c33-b112-73cc45c2ae2d\",\"ArtifactID\":123},{\"Name\":\"name\",\"ID\":\"423b4d43-eae9-4e14-b767-17d629de4bb2\",\"ArtifactID\":123}]",
                response.Content.ReadAsStringAsync().Result);
        }



    }
}
