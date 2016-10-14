using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using kCura.IntegrationPoints.Web.Attributes;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class ImportProviderDocumentController : ApiController
    {
        [HttpGet]
        [LogApiExceptionFilter(Message = "Unable to retrieve list of Ascii Delimiters.")]
        public IHttpActionResult GetAsciiDelimiters()
        {
            var asciiTable = WinEDDS.Utility.BuildProxyCharacterDatatable();

            return Json(asciiTable.Select().Select(x => x[0].ToString()));
        }
    }
}