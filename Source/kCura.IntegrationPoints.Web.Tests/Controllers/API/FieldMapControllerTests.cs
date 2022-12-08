using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Hosting;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Controllers.API;
using NSubstitute;
using NUnit.Framework;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Web.Tests.Controllers.API
{
    [TestFixture, Category("Unit")]
    public class FieldMapControllerTests : WebControllerTestBase
    {
        private FieldMapController _instance;
        private IIntegrationPointService _integrationPointReader;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _integrationPointReader = Substitute.For<IIntegrationPointService>();

            _instance = new FieldMapController(_integrationPointReader, Helper)
            {
                Request = new HttpRequestMessage()
            };
            _instance.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        public void ItShouldReturnAllFieldMaps(int numberOfElements)
        {
            //Arrange
            const int fieldMapId = 1234;
            List<FieldMap> expectedFieldMap = GenerateDefaultFieldMapList(numberOfElements, FieldMapTypeEnum.None);
            _integrationPointReader.GetFieldMap(fieldMapId).Returns(expectedFieldMap);

            //Act
            HttpResponseMessage response = _instance.Get(fieldMapId);
            List<FieldMap> actualResult = ExtractFieldMapListFromResponse(response);

            //Assert
            Assert.AreEqual(expectedFieldMap.Count, actualResult.Count);
            CollectionAssert.AreEqual(expectedFieldMap, actualResult);
        }

        [TestCase(0, 0)]
        [TestCase(0, 1)]
        [TestCase(1, 3)]
        [TestCase(10, 5)]
        public void ItShouldReturnFieldMapsWithoutFolderPathInformation(int numOfElements, int numOfFolderPathInfoFieldMaps)
        {
            //Arrange
            const int fieldMapId = 1234;
            List<FieldMap> expectedFieldMap = GenerateDefaultFieldMapList(numOfElements, FieldMapTypeEnum.None);
            List<FieldMap> extendedFieldMap = AppendFolderPathInformationFieldMaps(expectedFieldMap, numOfFolderPathInfoFieldMaps);
            _integrationPointReader.GetFieldMap(fieldMapId).Returns(extendedFieldMap);

            //Act
            HttpResponseMessage response = _instance.Get(fieldMapId);
            List<FieldMap> actualResult = ExtractFieldMapListFromResponse(response);

            //Assert
            Assert.AreEqual(numOfElements, actualResult.Count);
            CollectionAssert.AreEqual(expectedFieldMap, actualResult);
        }

        #region "Helpers"

        private static List<FieldMap> GenerateDefaultFieldMapList(int numberOfElements, FieldMapTypeEnum fieldMapTypeEnum)
        {
            var fieldMap = new List<FieldMap>();
            for (var i = 0; i < numberOfElements; i++)
            {
                fieldMap.Add(new FieldMap()
                {
                    SourceField = new FieldEntry(),
                    DestinationField = new FieldEntry(),
                    FieldMapType = fieldMapTypeEnum
                });
            }
            return fieldMap;
        }

        private static List<FieldMap> AppendFolderPathInformationFieldMaps(List<FieldMap> fieldMaps, int numberOfElements)
        {
            var result = new List<FieldMap>();
            result.AddRange(fieldMaps);
            List<FieldMap> fieldMapFolderPathInfo = GenerateDefaultFieldMapList(numberOfElements, FieldMapTypeEnum.FolderPathInformation);
            result.AddRange(fieldMapFolderPathInfo);
            return result;
        }

        private static List<FieldMap> ExtractFieldMapListFromResponse(HttpResponseMessage response)
        {
            var objectContent = response.Content as ObjectContent;
            var result = (List<FieldMap>)objectContent?.Value;
            return result;
        }

        #endregion
    }
}
