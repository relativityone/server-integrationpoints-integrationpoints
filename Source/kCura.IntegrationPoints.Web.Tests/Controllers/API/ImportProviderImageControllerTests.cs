using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Web.Controllers.API;
using Relativity.API;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Tests.Controllers.API
{
	[TestFixture, Category("ImportProvider")]
	public class ImportProviderImageControllerTests : TestBase
	{

		private ImportProviderImageController _controller;
		private IContextContainerFactory _contextContainerFactory;
		private IManagerFactory _managerFactory;
		private IFieldManager _fieldManager;
		private ICPHelper _helper;
		private IContextContainer _contextContainer;
		private ArtifactFieldDTO[] _fieldArray;

		[SetUp]
		public override void SetUp()
		{
			_contextContainerFactory = Substitute.For<IContextContainerFactory>();
			_managerFactory = Substitute.For<IManagerFactory>();
			_helper = Substitute.For<ICPHelper>();
			_contextContainer = Substitute.For<IContextContainer>();
			_fieldManager = Substitute.For<IFieldManager>();

			_contextContainerFactory.CreateContextContainer(_helper).Returns(_contextContainer);
			_managerFactory.CreateFieldManager(_contextContainer).Returns(_fieldManager);
			_controller = new ImportProviderImageController(_contextContainerFactory, _managerFactory, _helper);
		}

		[Test]
		public void ItShouldReturnOverlayFields()
		{
			//Arrange
			GenerateFieldArray();
			_fieldManager.RetrieveBeginBatesFields(123456).ReturnsForAnyArgs(_fieldArray);

			//Act
			IHttpActionResult response = _controller.GetOverlayIdentifierFields(123456);
			IEnumerable<ArtifactFieldDTO> actualResult = ExtractArrayResponse(response);

			//Assert
			CollectionAssert.AreEqual(_fieldArray, actualResult);
		}

		private void GenerateFieldArray()
		{
			List<ArtifactFieldDTO> fieldList = new List<ArtifactFieldDTO>();
			for (int i = 1; i < 6; i++)
			{
				fieldList.Add(new ArtifactFieldDTO { ArtifactId = i, Name = string.Format("FieldName{0}", i.ToString()) });
			}

			_fieldArray = fieldList.ToArray();
		}

		private ArtifactFieldDTO[] ExtractArrayResponse(IHttpActionResult response)
		{
			JsonResult<ArtifactFieldDTO[]> result = response as JsonResult<ArtifactFieldDTO[]>;
			return result.Content;
		}
	}
}
