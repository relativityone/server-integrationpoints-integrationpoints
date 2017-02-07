using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Web.Controllers.API;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using NSubstitute;
using NUnit.Framework;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Web.Tests.Controllers.API
{
	[TestFixture]
	public class ImportProviderDocumentControllerTests : TestBase
	{
		private int MAX_FIELDS = 100;
		private const string FIELD_NAME_BASE = "col-";

		private ImportProviderDocumentController _controller;
		private IFieldParserFactory _fieldParserFactory;
		private IFieldParser _fieldParser;
		private IImportTypeService _importTypeService;
		private ISerializer _serializer;
		private IRepositoryFactory _repositoryFactory;
		private IInstanceSettingRepository _instanceSettingRepo;

		[SetUp]
		public override void SetUp()
		{
			_importTypeService = Substitute.For<IImportTypeService>();
			_fieldParser = Substitute.For<IFieldParser>();
			_fieldParserFactory = Substitute.For<IFieldParserFactory>();
			_serializer = Substitute.For<ISerializer>();
			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_instanceSettingRepo = Substitute.For<IInstanceSettingRepository>();
			_controller = new ImportProviderDocumentController(_fieldParserFactory, _importTypeService, _serializer, _repositoryFactory);
		}

		[Test]
		public void ItShouldReturnAsciiDelimiterList()
		{
			//retrieve the ascii list in the format we expect
			IEnumerable<string> expectedResult = WinEDDS.Utility.BuildProxyCharacterDatatable().Select().Select(x => x[0].ToString());

			IHttpActionResult response = _controller.GetAsciiDelimiters();
			IEnumerable<string> actualResult = ExtractListResponse(response);

			CollectionAssert.AreEqual(expectedResult, actualResult);

		}

		[Test]
		public void ItShouldReturnLoadFileHeaders()
		{
			List<string> testHeaders = TestHeaders(new System.Random().Next(MAX_FIELDS));
			List<string> sortedHeaders = new List<string>(testHeaders);
			sortedHeaders.Sort();

			_fieldParserFactory.GetFieldParser(null).ReturnsForAnyArgs(_fieldParser);
			_fieldParser.GetFields().Returns(testHeaders);


			string actionResult = ExtractStringResponse(_controller.LoadFileHeaders(""));
			string[] splittedResult = actionResult.Split(new char[] { (char)13, (char)10 }, System.StringSplitOptions.RemoveEmptyEntries);

			IEnumerator<string> tdEnum = sortedHeaders.GetEnumerator();
			tdEnum.MoveNext();
			int idx = 0;
			foreach (string currentResult in splittedResult)
			{
				Assert.AreEqual(currentResult, string.Format("{0} ({1})", tdEnum.Current, testHeaders.IndexOf(tdEnum.Current) + 1));

				tdEnum.MoveNext();
				idx++;
			}
		}

		[TestCase("True")]
		[TestCase("False")]
		[TestCase(null)]
		public void ItShouldReturnCloudInstanceValue(string instanceSettingValue)
		{
			_repositoryFactory.GetInstanceSettingRepository().ReturnsForAnyArgs(_instanceSettingRepo);
			_instanceSettingRepo.GetConfigurationValue(Domain.Constants.RELATIVITY_CORE_SECTION, Domain.Constants.CLOUD_INSTANCE_NAME).ReturnsForAnyArgs(instanceSettingValue);
			_controller = new ImportProviderDocumentController(_fieldParserFactory, _importTypeService, _serializer, _repositoryFactory);

			JsonResult<string> result = _controller.IsCloudInstance() as JsonResult<string>;
			string isCloudInstance = result.Content;

			//Assert that we get the correct expected value based on the instance setting value
			string expectedValue;
			if (!string.IsNullOrEmpty(instanceSettingValue))
			{
				expectedValue = instanceSettingValue.ToLower();
			}
			else
			{
				expectedValue = "false";
			}

			Assert.AreEqual(expectedValue, isCloudInstance);
		}

		private IEnumerable<string> ExtractListResponse(IHttpActionResult response)
		{
			JsonResult<IEnumerable<string>> result = response as JsonResult<IEnumerable<string>>;
			return result.Content;
		}

		private string ExtractStringResponse(IHttpActionResult response)
		{
			return (response as OkNegotiatedContentResult<string>).Content;
		}

		private List<string> TestHeaders(int fieldCount)
		{
			return
				Enumerable
				.Range(0, fieldCount)
				.Select(x => string.Format(FIELD_NAME_BASE + "{0}", x))
				.ToList();
		}
	}
}
