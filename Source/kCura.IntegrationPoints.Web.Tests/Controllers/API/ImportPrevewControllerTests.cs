using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Results;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Controllers.API;
using kCura.IntegrationPoints.ImportProvider.Parser.Services.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Tests.Controllers.API
{
    [TestFixture, Category("Unit")]
    public class ImportPrevewControllerTests : TestBase
    {
        private ImportPreviewController _controller;
        private IImportPreviewService _service;

        [SetUp]
        public override void SetUp()
        {
            _service = Substitute.For<IImportPreviewService>();
            _controller = new ImportPreviewController(_service);
        }

        [Test]
        public void CreatePreviewReturnsJobId()
        {
            _service.CreatePreviewJob(new ImportPreviewSettings()).ReturnsForAnyArgs(1);

            IHttpActionResult responseMessage = _controller.CreatePreviewJob(new ImportPreviewSettings());
            int result = ExtractIntResponse(responseMessage);

            Assert.AreEqual(1, result);
        }

        [Test]
        public void GetPreviewTableShouldAddFirstColumn()
        {
            // Create a dummy Table
            ImportPreviewTable previewTable = new ImportPreviewTable();
            previewTable.Header.Clear();
            previewTable.Header.AddRange(new List<string> { "a", "b", "c", "d" });
            List<string> row;

            for (int i = 0; i < 5; i++)
            {
                row = new List<string> { "col a", "col b", "col c", "col d" };
                previewTable.Data.Add(row);
            }
            _service.RetrievePreviewTable(0).ReturnsForAnyArgs(previewTable);

            // Test that the controller takes our dummy table and adds a new first column
            IHttpActionResult responseMessage = _controller.GetImportPreviewTable(0);
            ImportPreviewTable result = ExtractTableResponse(responseMessage);

            // assert that our resulting header now has the "#" in the first column
            Assert.AreEqual(new List<string> { "#", "a", "b", "c", "d" }, result.Header);

            // assert that the first column contains each row's number
            int rowNumber = 1;
            foreach (List<string> resultRow in result.Data)
            {
                Assert.AreEqual(rowNumber.ToString(), resultRow[0]);
                rowNumber++;
            }

        }

        private int ExtractIntResponse(IHttpActionResult response)
        {
            JsonResult<int> result = response as JsonResult<int>;
            return result.Content;
        }

        private ImportPreviewTable ExtractTableResponse(IHttpActionResult response)
        {
            JsonResult<ImportPreviewTable> result = response as JsonResult<ImportPreviewTable>;
            return result.Content;
        }
    }
}
