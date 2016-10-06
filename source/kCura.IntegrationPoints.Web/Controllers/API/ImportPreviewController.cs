﻿using System.Collections.Generic;
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
        public IHttpActionResult CreatePreviewJob(int workspaceId)
        {
            //TODO: pass in Load file path (and other settings)
            int jobId = _importPreviewService.CreatePreviewJob(@"\\con-clar-rel02\FileShare\O365\test.txt", workspaceId);
            _importPreviewService.StartPreviewJob(jobId);
            return Json(jobId);
        }

        [HttpGet]
        public IHttpActionResult CheckProgress(int jobId)
        {
            ImportPreviewStatus progressData =_importPreviewService.CheckProgress(jobId);

            return Json(progressData);
        }

        [HttpGet]
        public IHttpActionResult GetImportPreviewTable(int jobId)
        {
            ImportPreviewTable previewTable = _importPreviewService.RetrievePreviewTable(jobId);

            previewTable.Header.Insert(0, "#");
            int rowNumber = 1;
            foreach(var row in previewTable.Data)
            {
                //Add a column that simply numbers each row
                row.Insert(0, string.Format("{0}",rowNumber++));
            }

            return Json(previewTable);
        }       
    }
}