using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class ImportProviderDocumentController : ApiController
    {
        [HttpGet]
        public IHttpActionResult GetAsciiDelimiters()
        {
            var asciiTable = WinEDDS.Utility.BuildProxyCharacterDatatable();

            return Json(asciiTable.Select().Select(x => x[0].ToString()));
        }
    }
}