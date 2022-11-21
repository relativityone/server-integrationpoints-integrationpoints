using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;
using kCura.IntegrationPoints.Core.Helpers;
using Relativity.API;
using SystemInterface.IO;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Contracts.Import;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class ImportProviderDocumentController : ApiController
    {
        private readonly IFieldParserFactory _fieldParserFactory;
        private readonly IImportTypeService _importTypeService;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly ISerializer _serializer;
        private readonly IImportFileLocationService _importFileLocationService;
        private readonly IFile _fileIo;
        private readonly IStreamFactory _streamFactory;
        private readonly IAPILog _logger;
        private readonly ICryptographyHelper _cryptographyHelper;
        private readonly IIntegrationPointService _integrationPointService;

        public ImportProviderDocumentController(IFieldParserFactory fieldParserFactory,
            IImportTypeService importTypeService,
            ISerializer serializer,
            IRepositoryFactory repoFactory,
            IImportFileLocationService importFileLocationService,
            IFile fileIo,
            IStreamFactory streamFactory,
            ICPHelper helper,
            ICryptographyHelper cryptographyHelper,
            IIntegrationPointService integrationPointService)
        {
            _fieldParserFactory = fieldParserFactory;
            _importTypeService = importTypeService;
            _serializer = serializer;
            _repositoryFactory = repoFactory;
            _importFileLocationService = importFileLocationService;
            _fileIo = fileIo;
            _streamFactory = streamFactory;

            _logger = helper.GetLoggerFactory().GetLogger();
            _cryptographyHelper = cryptographyHelper;
            _integrationPointService = integrationPointService;
        }

        [HttpGet]
        [LogApiExceptionFilter(Message = "Unable to retrieve list of Ascii Delimiters.")]
        public IHttpActionResult GetAsciiDelimiters()
        {
            var asciiTable = WinEDDS.Utility.BuildProxyCharacterDatatable();
            return Json(asciiTable.Select().Select(x => x[0].ToString()));
        }

        [HttpPost]
        [LogApiExceptionFilter(Message = "Unable to retrieve load file headers.")]
        public IHttpActionResult LoadFileHeaders([FromBody] string settings)
        {
            ImportProviderSettings providerSettings = _serializer.Deserialize<ImportProviderSettings>(settings);
            return Ok(string.Join(new string(new char[] { (char)13, (char)10 }),
                _fieldParserFactory.GetFieldParser(providerSettings).GetFields()
                .Select((name, i) => new { Name = name, Index = i + 1 })
                .OrderBy(x => x.Name)
                .Select(x => string.Format("{0} ({1})", x.Name, x.Index))));
        }

        [HttpGet]
        [LogApiExceptionFilter(Message = "Unable to retrieve list for Import Types.")]
        public IHttpActionResult GetImportTypes(bool isRdo = false)
        {
            return Json(_importTypeService.GetImportTypes(isRdo));
        }

        [HttpGet]
        [LogApiExceptionFilter(Message = "Unable to check cloud instance setting.")]
        public IHttpActionResult IsCloudInstance()
        {
            IInstanceSettingRepository instanceSettings =  _repositoryFactory.GetInstanceSettingRepository();
            string isCloudInstance = instanceSettings.GetConfigurationValue(Domain.Constants.RELATIVITY_CORE_SECTION, Domain.Constants.CLOUD_INSTANCE_NAME);
            if (string.IsNullOrEmpty(isCloudInstance))
            {
                isCloudInstance = "false";
            }
            return Json(isCloudInstance.ToLowerInvariant());
        }

        [HttpPost]
        [LogApiExceptionFilter(Message = "Unable to retrieve view data for import provider.")]
        public IHttpActionResult ViewData([FromBody] object data)
        {
            List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
            if (data != null)
            {
                string dataString = data.ToString();
                if (string.IsNullOrEmpty(dataString))
                {
                    result.Add(new KeyValuePair<string, string>("Source Location",
                            "No load file selected"));
                }
                else
                {
                    ImportSettingsBase settings = _serializer.Deserialize<ImportSettingsBase>(dataString);

                    result.Add(new KeyValuePair<string, string>("Source Location",
                            settings.LoadFile));
                    result.Add(new KeyValuePair<string, string>("Import Type",
                            System.Enum.GetName(typeof(ImportType.ImportTypeValue), int.Parse(settings.ImportType))));
                }
            }
            return Ok(result);
        }

        [HttpGet]
        [LogApiExceptionFilter(Message = "Unable to check error file location.")]
        public async Task<IHttpActionResult> CheckErrorFile(int artifactId, int workspaceId)
        {
            IntegrationPointDto integrationPoint = _integrationPointService.Read(artifactId);
            string errorFilePath = _importFileLocationService.ErrorFilePath(
                integrationPoint.ArtifactId,
                integrationPoint.Name,
                integrationPoint.SourceConfiguration,
                integrationPoint.DestinationConfiguration);

            if (_fileIo.Exists(errorFilePath))
            {
                return StatusCode(HttpStatusCode.NoContent);
            }
            else
            {
                _logger.LogWarning(
                    "Error file for integration point {ArtifactId} in workspace {WorkspaceId} was not present; expected path was {ErrorFilePath}",
                    artifactId, workspaceId, _cryptographyHelper.CalculateHash(errorFilePath));
                return BadRequest();
            }
        }

        [HttpGet]
        [LogApiExceptionFilter(Message = "Unable to retrieve error file for download.")]
        public async Task<IHttpActionResult> DownloadErrorFile(int artifactId, int workspaceId)
        {
            IntegrationPointDto integrationPoint = _integrationPointService.Read(artifactId);
            string errorFilePath = _importFileLocationService.ErrorFilePath(
                integrationPoint.ArtifactId,
                integrationPoint.Name,
                integrationPoint.SourceConfiguration,
                integrationPoint.DestinationConfiguration);

            byte[] trimmedBuffer = GetTrimmedBuffer(errorFilePath);

            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(trimmedBuffer)
            };
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = string.Concat("Error_file", Path.GetExtension(errorFilePath))
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            return ResponseMessage(result);
        }

        private byte[] GetTrimmedBuffer(string filePath)
        {
            IMemoryStream msWrap = _streamFactory.GetMemoryStream();
            using (IFileStream f = _streamFactory.GetFileStream(filePath))
            {
                f.CopyTo(msWrap);
            }

            //Cannot pass stream.GetBuffer() directly to ByteArrayContent ctor: MemoryStream's buffer has trailing NULLs.
            //http://stackoverflow.com/a/240745

            byte[] memStreamBuf = msWrap.GetBuffer();
            int i = msWrap.GetBuffer().Length - 1;
            while(memStreamBuf[i] == 0)
            {
                i--;
            }
            // now memStreamBuf[i] is the last non-zero byte
            byte[] rv = new byte[i+1];
            System.Array.Copy(memStreamBuf, rv, i+1);

            return rv;
        }
    }
}
