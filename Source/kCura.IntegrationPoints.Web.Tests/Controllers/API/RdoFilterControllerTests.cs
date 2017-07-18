using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Hosting;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Web.Controllers.API;
using kCura.Relativity.Client;
using NSubstitute;
using NUnit.Framework;
using Relativity.Core.Process;
using ObjectType = kCura.Relativity.Client.DTOs.ObjectType;

namespace kCura.IntegrationPoints.Web.Tests.Controllers.API
{
	[TestFixture]
	public class RdoFilterControllerTests : TestBase
	{
		private RdoFilterController _instance;
		private IRdoFilter _rdoFilter;
		private IRsapiRdoQuery _query;

		[SetUp]
		public override void SetUp()
		{
			_rdoFilter = Substitute.For<IRdoFilter>();
			_query = Substitute.For<IRsapiRdoQuery>();

			_instance = new RdoFilterController(_rdoFilter, _query)
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
			List<ObjectType> expectedViewableRdos = GenerateDefaultViewableRdosList(numOfViewableRdos);
			_rdoFilter.GetAllViewableRdos().Returns(expectedViewableRdos);

			//Act
			HttpResponseMessage response = _instance.GetAllViewableRdos();
			List<ObjectType> actualViewableRdos = ExtractViewableRdosListFromResponse(response);

			//Assert
			Assert.AreEqual(expectedViewableRdos.Count, actualViewableRdos.Count);
			for (var i = 0; i < numOfViewableRdos; i++)
			{
				Assert.AreEqual(expectedViewableRdos[i].Name, actualViewableRdos[i].Name);
				Assert.AreEqual(expectedViewableRdos[i].DescriptorArtifactTypeID, actualViewableRdos[i].DescriptorArtifactTypeID);
			}
		}

		[Test]
		public void ItShouldReturnRdoFilter()
		{
			//Arrange
			const int id = 1234;
			var expectedObject = new ObjectType()
			{
				DescriptorArtifactTypeID = 7987,
				Name = "ObjectTypeName"
			};
			_query.GetObjectType(id).Returns(expectedObject);

			//Act
			HttpResponseMessage response = _instance.Get(id);
			ObjectType actualResult = ExtractObjectTypeFromResponse(response);

			//Assert
			Assert.NotNull(actualResult);
			Assert.AreEqual(expectedObject.Name, actualResult.Name);
			Assert.AreEqual(expectedObject.DescriptorArtifactTypeID, actualResult.DescriptorArtifactTypeID);
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

		private static List<ObjectType> ExtractViewableRdosListFromResponse(HttpResponseMessage response)
		{

			var result = new List<ObjectType>();
			var objectContent = response.Content as ObjectContent;
			if (objectContent?.Value == null)
			{
				return null;
			}

			foreach (dynamic tmp in (IEnumerable) objectContent?.Value)
			{
				string name = tmp.name;
				var descriptorArtifactTypeId = (int)tmp.value;
				result.Add(new ObjectType()
				{
					Name = name,
					DescriptorArtifactTypeID = descriptorArtifactTypeId
				});
			}

			return result;
		}

		private static ObjectType ExtractObjectTypeFromResponse(HttpResponseMessage response)
		{
			var objectContent = response.Content as ObjectContent;
			dynamic value = objectContent?.Value;
			if(value == null) { return null;}
			string name = value.name;
			var descriptorArtifactTypeId = (int)value.value;

			return new ObjectType()
			{
				Name = name,
				DescriptorArtifactTypeID = descriptorArtifactTypeId
			};
		}

		private static List<ObjectType> GenerateDefaultViewableRdosList(int numOfViewableRdos)
		{
			var viewableRdos = new List<ObjectType>();

			for (var i = 0; i < numOfViewableRdos; i++)
			{
				viewableRdos.Add(new ObjectType() {
					Name = i.ToString(),
					DescriptorArtifactTypeID = i
				});
			}

			return viewableRdos;
		}

		#endregion
	}
}
