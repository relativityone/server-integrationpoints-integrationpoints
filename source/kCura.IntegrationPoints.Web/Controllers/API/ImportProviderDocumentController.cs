using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class ImportProviderDocumentController : ApiController
    {
        private IFieldParserFactory _fieldParserFactory;
	    private IImportTypeService _importTypeService;
		public ImportProviderDocumentController(IFieldParserFactory fieldParserFactory, IImportTypeService importTypeService)
		{
			_fieldParserFactory = fieldParserFactory;
			_importTypeService = importTypeService;
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
            return Ok(string.Join(new string(new char[] { (char)13, (char)10 }),
                _fieldParserFactory.GetFieldParser(settings).GetFields()
                .Select((name, i) => new { Name = name, Index = i + 1 })
                .OrderBy(x => x.Name)
                .Select(x => string.Format("{0} ({1})", x.Name, x.Index))));
        }

		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to retrieve list for Import Types.")]
		public IHttpActionResult GetImportTypes()
		{
			return Json(_importTypeService.GetImportTypes());
	    }

    }
}