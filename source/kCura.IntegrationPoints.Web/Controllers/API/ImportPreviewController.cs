using System.Collections.Generic;
using System.Web.Http;
using System.Net;
using System.Net.Http;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.ImportProvider.Parser.Services.Interfaces;


namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class ImportPreviewController : ApiController
    {
        private IImportPreviewService _importPreviewService;
        public ImportPreviewController(IImportPreviewService importPreviewService)
        {
            _importPreviewService = importPreviewService;
        }

        [HttpGet]
        public IHttpActionResult PreviewFiles(int workspaceId)
        {
            int jobId = _importPreviewService.CreatePreviewJob(@"\\con-clar-rel02\FileShare\O365\test.txt", workspaceId);
            _importPreviewService.StartPreviewJob(jobId);
            return Json(jobId);
        }

        [HttpGet]
        public IHttpActionResult CheckProgress(int jobId)
        {
            //TODO: CheckProgress should return domain object w/ TotalBytes and IsComplete
            var progressData=_importPreviewService.CheckProgress(jobId);

            return Json(progressData);
        }

        [HttpGet]
        public IHttpActionResult GetImportPreviewTable(int jobId)
        {
            ImportPreviewTable previewTable = _importPreviewService.RetrievePreviewTable(jobId);

            return Json(previewTable);
        }

        [HttpGet]
        public IHttpActionResult DummbyData()
        {
            var model = new
            {
                Headers = new List<string>
                {
                    "F1",
                    "F2",
                    "Control Number",
                    "Custodian",
                    "Send Date",
                    "Send BCC",
                    "Send CC",
                    "Subject",
                    "Extracted Text"
                },
                Data = new List<List<string>>
                {
                    new List<string>
                    {
                        "Data1",
                        "Data2",
                        "Data3",
                        "Data4",
                        "Data5",
                        "Data6",
                        "Data7",
                        "Data8",
                        "Data9",
                    },
                    new List<string>
                    {
                        "Data10",
                        "Data11",
                        "Data12",
                        "Data13",
                        "Data14",
                        "Data15",
                        "Data16",
                        "Data17",
                        "Data18"
                    },
                    new List<string>
                    {
                        "Data19",
                        "Data20",
                        "Data21",
                        "Data22",
                        "Data23",
                        "Data24",
                        "Data25",
                        "Data26",
                        "Data27"
                    },

                    new List<string>
                    {
                        "Data28",
                        "Data29",
                        "Data30",
                        "Data31",
                        "Data32",
                        "Data33",
                        "Data34",
                        "Data35",
                        "Data36"
                    },
                    new List<string>
                    {
                        "Data37",
                        "Data38",
                        "Data39",
                        "Data40",
                        "Data41",
                        "Data42",
                        "Data43",
                        "Data44",
                        "Data45"
                    },
                    new List<string>
                    {
                        "Data46",
                        "Data47",
                        "Data48",
                        "Data49",
                        "Data50",
                        "Data51",
                        "Data52",
                        "Data53",
                        "Data54"
                    },
                    new List<string>
                    {
                        "Data55",
                        "Data56",
                        "Data57",
                        "Data58",
                        "Data59",
                        "Data60",
                        "Data61",
                        "Data62",
                        "Data63"
                    },
                    new List<string>
                    {
                        "Data64",
                        "Data65",
                        "Data66",
                        "Data67",
                        "Data68",
                        "Data70",
                        "Data71",
                        "Data72",
                        "Data73"
                    },
                    new List<string>
                    {
                        "Data74",
                        "Data75",
                        "Data76",
                        "Data77",
                        "Data78",
                        "Data79",
                        "Data80",
                        "Data81",
                        "Data82"
                    }
                }

            };

            return Json(model);

        }
    }
}