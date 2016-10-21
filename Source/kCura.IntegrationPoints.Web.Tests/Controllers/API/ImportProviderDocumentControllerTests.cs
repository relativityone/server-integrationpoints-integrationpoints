using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Http.Hosting;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Controllers.API;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Tests.Controllers.API
{
    public class ImportProviderDocumentControllerTests
    {
        private ImportProviderDocumentController _controller;
        private IFieldParserFactory _fieldParserFactory;

        [SetUp]
        public void SetUp()
        {
            _fieldParserFactory = Substitute.For<IFieldParserFactory>();
            _controller = new ImportProviderDocumentController(_fieldParserFactory);
        }

        [Test]
        public void ItShouldReturnAsciiDelimiterList()
        {
            //retrieve the ascii list in the format we expect
            IEnumerable<string> expectedResult= WinEDDS.Utility.BuildProxyCharacterDatatable().Select().Select(x => x[0].ToString());

            IHttpActionResult response = _controller.GetAsciiDelimiters();
            IEnumerable<string> actualResult = ExtractListResponse(response);

            CollectionAssert.AreEqual(expectedResult, actualResult);
            
        }

        private IEnumerable<string> ExtractListResponse(IHttpActionResult response)
        {
            JsonResult<IEnumerable<string>> result = response as JsonResult<IEnumerable<string>>;
            return result.Content;
        }
    }
}
