using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using kCura.IntegrationPoints.Web.Attributes;

using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class ImportProviderDocumentController : ApiController
    {
        private IFieldParserFactory _fieldParserFactory;
        public ImportProviderDocumentController(IFieldParserFactory fieldParserFactory)
        {
            _fieldParserFactory = fieldParserFactory;
        }

        [HttpGet]
        [LogApiExceptionFilter(Message = "Unable to retrieve list of Ascii Delimiters.")]
        public IHttpActionResult GetAsciiDelimiters()
        {
            var asciiTable = WinEDDS.Utility.BuildProxyCharacterDatatable();

            return Json(asciiTable.Select().Select(x => x[0].ToString()));
        }

        [HttpPost]
        public IHttpActionResult LoadFileHeaders([FromBody] string settings)
        {
            return Ok(settings);
        }

    }
}