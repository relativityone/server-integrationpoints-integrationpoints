using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Web.Controllers.API;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using NSubstitute;
using NUnit.Framework;
using kCura.IntegrationPoints.Core.Helpers;
using Relativity.API;
using SystemInterface.IO;
using kCura.IntegrationPoints.Core.Contracts.Import;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;

namespace kCura.IntegrationPoints.Web.Tests.Controllers.API
{
    [TestFixture, Category("Unit")]
    public class ImportProviderDocumentControllerTests : TestBase
    {
        private int _MAX_FIELDS = 100;
        private const string _FIELD_NAME_BASE = "col-";
        private readonly byte[] _FILE_CONTENT_MEM_STREAM_BYTES = new byte[] { 0x48, 0x65, 0x6c, 0x6c, 0x6f, 0, 0, 0 }; //"Hello" with some trailing null bytes
        private const int _FILE_CONTENT_VALID_BYTE_COUNT = 5;

        private ImportProviderDocumentController _controller;
        private IFieldParserFactory _fieldParserFactory;
        private IFieldParser _fieldParser;
        private IImportTypeService _importTypeService;
        private ISerializer _serializer;
        private IRepositoryFactory _repositoryFactory;
        private IInstanceSettingRepository _instanceSettingRepo;
        private IImportFileLocationService _importLocationService;
        private IFile _fileIo;
        private IMemoryStream _memoryStream;
        private IFileStream _fileStream;
        private IStreamFactory _streamFactory;
        private ICPHelper _helper;
        private ILogFactory _logFactory;
        private IAPILog _logger;
        private ICryptographyHelper _cryptographyHelper;
        private IIntegrationPointService _integrationPointService;

        [SetUp]
        public override void SetUp()
        {
            _importTypeService = Substitute.For<IImportTypeService>();
            _fieldParser = Substitute.For<IFieldParser>();
            _fieldParserFactory = Substitute.For<IFieldParserFactory>();
            _serializer = Substitute.For<ISerializer>();
            _repositoryFactory = Substitute.For<IRepositoryFactory>();
            _instanceSettingRepo = Substitute.For<IInstanceSettingRepository>();
            _importLocationService = Substitute.For<IImportFileLocationService>();
            _fileIo = Substitute.For<IFile>();
            _memoryStream = Substitute.For<IMemoryStream>();
            _fileStream = Substitute.For<IFileStream>();
            _streamFactory = Substitute.For<IStreamFactory>();
            _helper = Substitute.For<ICPHelper>();
            _logFactory = Substitute.For<ILogFactory>();
            _logger = Substitute.For<IAPILog>();

            _streamFactory.GetFileStream(Arg.Any<string>()).Returns(_fileStream);
            _streamFactory.GetMemoryStream().Returns(_memoryStream);
            _helper.GetLoggerFactory().Returns(_logFactory);
            _logFactory.GetLogger().Returns(_logger);
            _cryptographyHelper = Substitute.For<ICryptographyHelper>();

            _integrationPointService = Substitute.For<IIntegrationPointService>();
            _integrationPointService.Read(Arg.Any<int>()).Returns(new IntegrationPointDto());

            _controller = new ImportProviderDocumentController(_fieldParserFactory,
                _importTypeService,
                _serializer,
                _repositoryFactory,
                _importLocationService,
                _fileIo,
                _streamFactory,
                _helper,
                _cryptographyHelper,
                _integrationPointService);
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
            List<string> testHeaders = TestHeaders(new System.Random().Next(_MAX_FIELDS));
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
            _controller = new ImportProviderDocumentController(_fieldParserFactory,
                _importTypeService,
                _serializer,
                _repositoryFactory,
                _importLocationService,
                _fileIo,
                _streamFactory,
                _helper,
                _cryptographyHelper,
                _integrationPointService);

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

        [Test]
        public void ItShouldReturnNoContentResultIfErrorFileExists()
        {
            _importLocationService.ErrorFilePath(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(string.Empty);
            _fileIo.Exists(Arg.Any<string>()).Returns(true);

            IHttpActionResult result = _controller.CheckErrorFile(-1, -1);

            Assert.IsInstanceOf(typeof(StatusCodeResult), result);
            StatusCodeResult statusCodeResult = result as StatusCodeResult;
            Assert.AreEqual(HttpStatusCode.NoContent, statusCodeResult?.StatusCode);
        }

        [Test]
        public void ItShouldReturnBadRequestResultIfErrorFileMissing()
        {
            _importLocationService.ErrorFilePath(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(string.Empty);
            _fileIo.Exists(Arg.Any<string>()).Returns(false);

            IHttpActionResult result = _controller.CheckErrorFile(-1, -1);

            Assert.IsInstanceOf(typeof(BadRequestResult), result);
        }

        [Test]
        public void ItShouldReturnCorrectResponseMessageResultForDownload()
        {
            _importLocationService.ErrorFilePath(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(string.Empty);
            _memoryStream.GetBuffer().Returns(_FILE_CONTENT_MEM_STREAM_BYTES);

            IHttpActionResult result = _controller.DownloadErrorFile(-1, -1);
            ResponseMessageResult responseResult = (ResponseMessageResult)result;
            Task<byte[]> t = responseResult.Response.Content.ReadAsByteArrayAsync();
            t.Wait();
            byte[] prunedBytes = t.Result;

            Assert.IsInstanceOf(typeof(ResponseMessageResult), result);
            Assert.AreEqual(System.Net.HttpStatusCode.OK, responseResult.Response.StatusCode);
            Assert.AreEqual(_FILE_CONTENT_VALID_BYTE_COUNT, prunedBytes.Length);
            for (int i = 0; i < _FILE_CONTENT_VALID_BYTE_COUNT; i++)
            {
                Assert.AreEqual(_FILE_CONTENT_MEM_STREAM_BYTES[i], prunedBytes[i]);
            }
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
                .Select(x => string.Format(_FIELD_NAME_BASE + "{0}", x))
                .ToList();
        }
    }
}
