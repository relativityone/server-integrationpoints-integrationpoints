using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Hosting;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Controllers.API;
using kCura.IntegrationPoints.Web.Extensions;
using Moq;
using NUnit.Framework;
using Relativity;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Tests.Controllers.API
{
    [TestFixture, Category("Unit")]
    public class RdoFilterControllerTests : TestBase
    {
        private Mock<ICaseServiceContext> _caseServiceContext;
        private Mock<IRdoFilter> _rdoFilter;
        private Mock<IObjectTypeRepository> _objectTypeRepository;
        private Mock<IHelper> _helper;

        private RdoFilterController _sut;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _caseServiceContext = new Mock<ICaseServiceContext>();
            _rdoFilter = new Mock<IRdoFilter>();
            _objectTypeRepository = new Mock<IObjectTypeRepository>();
            _helper = new Mock<IHelper>();
            _helper.Setup(x => x.GetLoggerFactory().GetLogger().ForContext<RdoFilterController>()).Returns(new Mock<IAPILog>().Object);

            _sut = new RdoFilterController(_caseServiceContext.Object, _rdoFilter.Object, _objectTypeRepository.Object, _helper.Object)
            {
                Request = new HttpRequestMessage()
            };

            _sut.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            HttpRuntime.Cache.Clear();
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(5)]
        public void GetAllViewableRdos_ShouldReturnAllViewableRdos(int numOfViewableRdos)
        {
            //Arrange 
            List<ObjectTypeDTO> expectedViewableRdos = GenerateDefaultViewableRdosList(numOfViewableRdos);
            _rdoFilter.Setup(x => x.GetAllViewableRdosAsync()).ReturnsAsync(expectedViewableRdos);

            //Act
            HttpResponseMessage response = _sut.GetAllViewableRdos();
            List<ObjectTypeDTO> actualViewableRdos = ExtractViewableRdosListFromResponse(response);

            //Assert
            Assert.AreEqual(expectedViewableRdos.Count, actualViewableRdos.Count);
            for (var i = 0; i < numOfViewableRdos; i++)
            {
                Assert.AreEqual(expectedViewableRdos[i].Name, actualViewableRdos[i].Name);
                Assert.AreEqual(expectedViewableRdos[i].DescriptorArtifactTypeId, actualViewableRdos[i].DescriptorArtifactTypeId);
            }
        }

        [Test]
        public void GetAllViewableRdos_ShouldReturnFromCache()
        {
            //Arrange 
            int workspaceId = 2;
            int userId = 3;

            _caseServiceContext.SetupGet(x => x.WorkspaceID).Returns(workspaceId);
            _caseServiceContext.SetupGet(x => x.WorkspaceUserID).Returns(userId);

            //Act
            _sut.GetAllViewableRdos();
            _sut.GetAllViewableRdos();

            //Assert
            _rdoFilter.Verify(x => x.GetAllViewableRdosAsync(), Times.Once);
        }

        [Test]
        public async Task GetAllViewableRdos_ShouldNotReturnFromCache_WhenCacheIsExpired()
        {
            //Arrange 
            int workspaceId = 2;
            int userId = 3;

            _caseServiceContext.SetupGet(x => x.WorkspaceID).Returns(workspaceId);
            _caseServiceContext.SetupGet(x => x.WorkspaceUserID).Returns(userId);

            _sut.ViewableRdosCacheSlidingExpirationTimeout = TimeSpan.FromMilliseconds(1);

            //Act
            _sut.GetAllViewableRdos();
            await Task.Delay(TimeSpan.FromMilliseconds(5));
            _sut.GetAllViewableRdos();

            //Assert
            _rdoFilter.Verify(x => x.GetAllViewableRdosAsync(), Times.Exactly(2));
        }

        [Test]
        public void GetAllViewableRdos_ShouldNotReturnFromCache_WhenWorkspaceIsDifferent()
        {
            //Arrange 
            int firstWorkspaceId = 2;
            int secondWorkspaceId = 3;
            int userId = 9;

            _caseServiceContext.SetupGet(x => x.WorkspaceID).Returns(firstWorkspaceId);
            _caseServiceContext.SetupGet(x => x.WorkspaceUserID).Returns(userId);

            //Act
            _sut.GetAllViewableRdos();
            _caseServiceContext.SetupGet(x => x.WorkspaceID).Returns(secondWorkspaceId);
            _sut.GetAllViewableRdos();

            //Assert
            _rdoFilter.Verify(x => x.GetAllViewableRdosAsync(), Times.Exactly(2));
        }

        [Test]
        public void GetAllViewableRdos_ShouldNotReturnFromCache_WhenUserIdIsDifferent()
        {
            //Arrange 
            int workspaceId = 2;
            int firstUserId = 9;
            int secondUserId = 10;

            _caseServiceContext.SetupGet(x => x.WorkspaceID).Returns(workspaceId);
            _caseServiceContext.SetupGet(x => x.WorkspaceUserID).Returns(firstUserId);

            //Act
            _sut.GetAllViewableRdos();
            _caseServiceContext.SetupGet(x => x.WorkspaceUserID).Returns(secondUserId);
            _sut.GetAllViewableRdos();

            //Assert
            _rdoFilter.Verify(x => x.GetAllViewableRdosAsync(), Times.Exactly(2));
        }

        [Test]
        public void Get_ShouldReturnRdoFilter()
        {
            //Arrange
            const int id = 1234;
            var expectedObject = new ObjectTypeDTO
            {
                DescriptorArtifactTypeId = 7987,
                Name = "ObjectTypeName"
            };
            _objectTypeRepository.Setup(x => x.GetObjectType(id)).Returns(expectedObject);

            //Act
            HttpResponseMessage response = _sut.Get(id);
            ObjectTypeDTO actualResult = ExtractObjectTypeFromResponse(response);

            //Assert
            Assert.NotNull(actualResult);
            Assert.AreEqual(expectedObject.Name, actualResult.Name);
            Assert.AreEqual(expectedObject.DescriptorArtifactTypeId, actualResult.DescriptorArtifactTypeId);
        }

        [Test]
        public void GetDefaultRdoTypeId_ShouldReturnDefaultRdoTypeId()
        {
            //Act
            HttpResponseMessage response = _sut.GetDefaultRdoTypeId();
            
            //Assert
            var objectContent = response.Content as ObjectContent;
            Assert.NotNull(objectContent?.Value);
            var result = (int)objectContent?.Value;

            Assert.AreEqual((int)ArtifactType.Document, result);
        }

        #region "Helpers"

        private static List<ObjectTypeDTO> ExtractViewableRdosListFromResponse(HttpResponseMessage response)
        {
            var result = new List<ObjectTypeDTO>();
            var objectContent = response.Content as ObjectContent;

            if (objectContent?.Value == null)
            {
                return null;
            }

            foreach (RdoFilterController.ViewableRdo tmp in (IEnumerable) objectContent?.Value)
            {
                string name = tmp.Name;
                var descriptorArtifactTypeId = (int)tmp.Value;
                result.Add(new ObjectTypeDTO
                {
                    Name = name,
                    DescriptorArtifactTypeId = descriptorArtifactTypeId
                });
            }

            return result;
        }

        private static ObjectTypeDTO ExtractObjectTypeFromResponse(HttpResponseMessage response)
        {
            var objectContent = response.Content as ObjectContent;
            dynamic value = objectContent?.Value;
            if(value == null) { return null;}
            string name = value.name;
            var descriptorArtifactTypeId = (int)value.value;

            return new ObjectTypeDTO
            {
                Name = name,
                DescriptorArtifactTypeId = descriptorArtifactTypeId
            };
        }

        private static List<ObjectTypeDTO> GenerateDefaultViewableRdosList(int numOfViewableRdos)
        {
            var viewableRdos = new List<ObjectTypeDTO>();

            for (var i = 0; i < numOfViewableRdos; i++)
            {
                viewableRdos.Add(new ObjectTypeDTO {
                    Name = i.ToString(),
                    DescriptorArtifactTypeId = i
                });
            }

            return viewableRdos;
        }

        #endregion
    }
}
