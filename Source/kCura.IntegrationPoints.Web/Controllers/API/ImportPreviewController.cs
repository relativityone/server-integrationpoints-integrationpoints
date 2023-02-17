using System.Collections.Generic;
using System.Web.Http;
using System.Net;
using System.Net.Http;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.ImportProvider.Parser.Services.Interfaces;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class ImportPreviewController : ApiController
    {
        private readonly IImportPreviewService _importPreviewService;

        public ImportPreviewController(IImportPreviewService importPreviewService)
        {
            _importPreviewService = importPreviewService;
        }

        [HttpPost]
        [LogApiExceptionFilter(Message = "Unable to create Preview Job")]
        public IHttpActionResult CreatePreviewJob([FromBody] ImportPreviewSettings settings)
        {
            int jobId = _importPreviewService.CreatePreviewJob(settings);
            _importPreviewService.StartPreviewJob(jobId);
            return Json(jobId);
        }

        [HttpGet]
        [LogApiExceptionFilter(Message = "Unable to check the progress of the given Preview Job")]
        public IHttpActionResult CheckProgress(int jobId)
        {
            ImportPreviewStatus progressData =_importPreviewService.CheckProgress(jobId);

            return Json(progressData);
        }

        [HttpGet]
        [LogApiExceptionFilter(Message = "Unable to retrieve the Table for the given Preview Job")]
        public IHttpActionResult GetImportPreviewTable(int jobId)
        {
            ImportPreviewTable previewTable = _importPreviewService.RetrievePreviewTable(jobId);

            previewTable.Header.Insert(0, "#");
            int rowNumber = 1;
            foreach (var row in previewTable.Data)
            {
                // Add a column that simply numbers each row
                row.Insert(0, string.Format("{0}",rowNumber++));
            }

            return Json(previewTable);
        }
    }
}
