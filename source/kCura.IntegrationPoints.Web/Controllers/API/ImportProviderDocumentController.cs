using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class ImportProviderDocumentController : ApiController
	{
		private IFieldParserFactory _fieldParserFactory;
		private IImportTypeService _importTypeService;
		private IRepositoryFactory _repositoryFactory;
		private ISerializer _serializer;

		public ImportProviderDocumentController(IFieldParserFactory fieldParserFactory, IImportTypeService importTypeService, ISerializer serializer, IRepositoryFactory repoFactory)
		{
			_fieldParserFactory = fieldParserFactory;
			_importTypeService = importTypeService;
			_serializer = serializer;
			_repositoryFactory = repoFactory;
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
			return Json(isCloudInstance.ToLower());
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
	}
}