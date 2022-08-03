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
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Services.SourceTypes;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Web.Controllers.API;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts;
using Relativity.IntegrationPoints.Contracts.Internals;

namespace kCura.IntegrationPoints.Web.Tests.Controllers
{
    [TestFixture, Category("Unit")]
    public class SourceTypeControllerTests : WebControllerTestBase
    {
        private SourceTypeController _instance;
        private IWindsorContainer _windsorContainer;
        private ISourceTypeFactory _sourceTypeFactory;
        private ICaseServiceContext _iCaseServiceContext;
        private IObjectTypeRepository _objTypeQuery;
        private Guid _documentObjectGuid;
        private Guid _randomRdoGuid;

        private void SetUpWindsorContainer()
        {
            _windsorContainer.Register(Component.For<ISourceTypeFactory>().Instance(_sourceTypeFactory).LifestyleTransient());
            _windsorContainer.Register(Component.For<SourceTypeController>());
            _windsorContainer.Register(Component.For<ICaseServiceContext>().Instance(_iCaseServiceContext).LifestyleTransient());
            _windsorContainer.Register(Component.For<IObjectTypeRepository>().Instance(_objTypeQuery).LifestyleTransient());
            _windsorContainer.Register(Component.For<ICPHelper>().UsingFactoryMethod((k) => Helper).LifestyleTransient());
        }

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _windsorContainer = new WindsorContainer();
            _sourceTypeFactory = NSubstitute.Substitute.For<ISourceTypeFactory>();
            _iCaseServiceContext = NSubstitute.Substitute.For<ICaseServiceContext>();
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
            _instance = _windsorContainer.Kernel.Resolve<SourceTypeController>();
            _instance.ControllerContext = new HttpControllerContext(config, routeData, request);
            _instance.Request = request;
            _instance.Request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;

        }



        [Test]
        public void Get_GoldFlow()
        {
            // Arrange
            IEnumerable<SourceType> sourceTypeModels = new List<SourceType>()
            {
                new SourceType()
                {
                    Name = "name",
                    ID = "d39d9a5e-e009-4c33-b112-73cc45c2ae2d", // some random guid
                    ArtifactID = 123,
                    SourceURL = "url"
                },
                new SourceType()
                {
                    Name = "name",
                    ID = Core.Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID,
                    ArtifactID = 123,
                    SourceURL = "url",
                    Config = new SourceProviderConfiguration()
                    {
                        CompatibleRdoTypes = new List<Guid>() { _documentObjectGuid },
                        AvailableImportSettings = new ImportSettingVisibility()
                        {
                            AllowUserToMapNativeFileField = false
                        }
                    }
                }
            };

            _sourceTypeFactory.GetSourceTypes().Returns(sourceTypeModels);

            // Act
            HttpResponseMessage response = _instance.Get();

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            StringAssert.AreEqualIgnoringCase(
                "[{\"name\":\"name\",\"id\":123,\"value\":\"d39d9a5e-e009-4c33-b112-73cc45c2ae2d\",\"url\":\"url\",\"config\":{\"CompatibleRdoTypes\":null,\"OnlyMapIdentifierToIdentifier\":false,\"ImportSettingVisibility\":{\"AllowUserToMapNativeFileField\":true}}}" +
                ",{\"name\":\"name\",\"id\":123,\"value\":\"423b4d43-eae9-4e14-b767-17d629de4bb2\",\"url\":\"url\",\"config\":{\"CompatibleRdoTypes\":[10],\"OnlyMapIdentifierToIdentifier\":false,\"ImportSettingVisibility\":{\"AllowUserToMapNativeFileField\":false}}}]",
                response.Content.ReadAsStringAsync().Result);
        }

        [Test]
        public void Get_GoldFlow_NoMatchRdoTypes()
        {
            // Arrange
            IEnumerable<SourceType> sourceTypeModels = new List<SourceType>()
            {
                new SourceType()
                {
                    Name = "name",
                    ID = Core.Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID,
                    ArtifactID = 123,
                    SourceURL = "url",
                    Config = new SourceProviderConfiguration()
                    {
                        CompatibleRdoTypes = new List<Guid>() { new Guid(Core.Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID) }
                    }
                }
            };

            _sourceTypeFactory.GetSourceTypes().Returns(sourceTypeModels);

            // Act
            HttpResponseMessage response = _instance.Get();

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            StringAssert.AreEqualIgnoringCase(
                "[{\"name\":\"name\",\"id\":123,\"value\":\"423b4d43-eae9-4e14-b767-17d629de4bb2\",\"url\":\"url\",\"config\":{\"CompatibleRdoTypes\":[],\"OnlyMapIdentifierToIdentifier\":false,\"ImportSettingVisibility\":{\"AllowUserToMapNativeFileField\":true}}}]",
                response.Content.ReadAsStringAsync().Result);
        }

        [Test]
        public void Get_GoldFlow_NoMatchRdoTypes_ConfigNotNull()
        {
            // Arrange
            IEnumerable<SourceType> sourceTypeModels = new List<SourceType>()
            {
                new SourceType()
                {
                    Name = "name",
                    ID = Core.Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID,
                    ArtifactID = 123,
                    SourceURL = "url",
                    Config = new SourceProviderConfiguration()
                }
            };

            _sourceTypeFactory.GetSourceTypes().Returns(sourceTypeModels);

            // Act
            HttpResponseMessage response = _instance.Get();

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            StringAssert.AreEqualIgnoringCase(
                "[{\"name\":\"name\",\"id\":123,\"value\":\"423b4d43-eae9-4e14-b767-17d629de4bb2\",\"url\":\"url\",\"config\":{\"CompatibleRdoTypes\":null,\"OnlyMapIdentifierToIdentifier\":false,\"ImportSettingVisibility\":{\"AllowUserToMapNativeFileField\":true}}}]",
                response.Content.ReadAsStringAsync().Result);
        }

        [Test]
        public void Get_GoldFlow_NoMatchRdoTypes_CompatibleRdoTypesNotNull()
        {
            // Arrange
            IEnumerable<SourceType> sourceTypeModels = new List<SourceType>()
            {
                new SourceType()
                {
                    Name = "name",
                    ID = Core.Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID,
                    ArtifactID = 123,
                    SourceURL = "url",
                    Config = new SourceProviderConfiguration()
                    {
                        CompatibleRdoTypes = new List<Guid>()
                    }
                }
            };

            _sourceTypeFactory.GetSourceTypes().Returns(sourceTypeModels);

            // Act
            HttpResponseMessage response = _instance.Get();

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            StringAssert.AreEqualIgnoringCase(
                "[{\"name\":\"name\",\"id\":123,\"value\":\"423b4d43-eae9-4e14-b767-17d629de4bb2\",\"url\":\"url\",\"config\":{\"CompatibleRdoTypes\":[],\"OnlyMapIdentifierToIdentifier\":false,\"ImportSettingVisibility\":{\"AllowUserToMapNativeFileField\":true}}}]",
                response.Content.ReadAsStringAsync().Result);
        }

        [Test]
        public void Get_GoldFlow_NoMatchRdoTypes_CompatibleToMultipleRdos()
        {
            // Arrange
            IEnumerable<SourceType> sourceTypeModels = new List<SourceType>()
            {
                new SourceType()
                {
                    Name = "name",
                    ID = Core.Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID,
                    ArtifactID = 123,
                    SourceURL = "url",
                    Config = new SourceProviderConfiguration()
                    {
                        CompatibleRdoTypes = new List<Guid>()
                        {
                            _documentObjectGuid,
                            _randomRdoGuid
                        }
                    }
                }
            };

            _sourceTypeFactory.GetSourceTypes().Returns(sourceTypeModels);

            // Act
            HttpResponseMessage response = _instance.Get();

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            StringAssert.AreEqualIgnoringCase(
                "[{\"name\":\"name\",\"id\":123,\"value\":\"423b4d43-eae9-4e14-b767-17d629de4bb2\",\"url\":\"url\",\"config\":{\"CompatibleRdoTypes\":[10,789456],\"OnlyMapIdentifierToIdentifier\":false,\"ImportSettingVisibility\":{\"AllowUserToMapNativeFileField\":true}}}]",
                response.Content.ReadAsStringAsync().Result);
        }
    }
}