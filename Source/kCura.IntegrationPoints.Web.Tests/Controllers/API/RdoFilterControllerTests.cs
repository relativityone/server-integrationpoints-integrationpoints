using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Hosting;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Controllers.API;
using NSubstitute;
using NUnit.Framework;
using Relativity;

namespace kCura.IntegrationPoints.Web.Tests.Controllers.API
{
    [TestFixture, Category("Unit")]
	public class RdoFilterControllerTests : TestBase
	{
		private RdoFilterController _instance;
		private IRdoFilter _rdoFilter;
		private IObjectTypeRepository _objectTypeRepository;

		[SetUp]
		public override void SetUp()
		{
			_rdoFilter = Substitute.For<IRdoFilter>();
			_objectTypeRepository = Substitute.For<IObjectTypeRepository>();

			_instance = new RdoFilterController(_rdoFilter, _objectTypeRepository)
			{
				Request = new HttpRequestMessage()
			};
			_instance.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
		}

		[TestCase(0)]
		[TestCase(1)]
		[TestCase(5)]
		public void ItShouldReturnAllViewableRdos(int numOfViewableRdos)
		{
			//Arrange 
			List<ObjectTypeDTO> expectedViewableRdos = GenerateDefaultViewableRdosList(numOfViewableRdos);
			_rdoFilter.GetAllViewableRdos().Returns(expectedViewableRdos);

			//Act
			HttpResponseMessage response = _instance.GetAllViewableRdos();
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
		public void ItShouldReturnRdoFilter()
		{
			//Arrange
			const int id = 1234;
			var expectedObject = new ObjectTypeDTO
			{
				DescriptorArtifactTypeId = 7987,
				Name = "ObjectTypeName"
			};
			_objectTypeRepository.GetObjectType(id).Returns(expectedObject);

			//Act
			HttpResponseMessage response = _instance.Get(id);
			ObjectTypeDTO actualResult = ExtractObjectTypeFromResponse(response);

			//Assert
			Assert.NotNull(actualResult);
			Assert.AreEqual(expectedObject.Name, actualResult.Name);
			Assert.AreEqual(expectedObject.DescriptorArtifactTypeId, actualResult.DescriptorArtifactTypeId);
		}

		[Test]
		public void ItShouldReturnDefaultRdoTypeId()
		{
			//Act
			HttpResponseMessage response = _instance.GetDefaultRdoTypeId();
			
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

			foreach (dynamic tmp in (IEnumerable) objectContent?.Value)
			{
				string name = tmp.name;
				var descriptorArtifactTypeId = (int)tmp.value;
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
