using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;
using Relativity.Services.ServiceProxy;

namespace Relativity.Sync.Tests.System.Helpers
{
	internal sealed class ImportHelper
	{
		private const int _CONTROL_NUMBER_FIELD_ARTIFACT_ID = 1003667;

		private readonly ServiceFactory _serviceFactory;

		public ImportHelper(ServiceFactory serviceFactory)
		{
			_serviceFactory = serviceFactory;
		}

		public async Task<ImportJobResult> ImportDataAsync(int workspaceArtifactId, ImportDataTableWrapper dataTableWrapper)
		{
			kCura.WinEDDS.Config.ConfigSettings[nameof(kCura.WinEDDS.Config.TapiForceHttpClient)] = true.ToString(CultureInfo.InvariantCulture);
			kCura.WinEDDS.Config.ConfigSettings[nameof(kCura.WinEDDS.Config.TapiForceBcpHttpClient)] = true.ToString(CultureInfo.InvariantCulture);

			var importApi =
				new ImportAPI(
					AppSettings.RelativityUserName,
					AppSettings.RelativityUserPassword,
					AppSettings.RelativityWebApiUrl.ToString());

			ImportJobResult result = await ConfigureAndRunImportApiJobAsync(workspaceArtifactId, dataTableWrapper, importApi).ConfigureAwait(false);

			return result;
		}

		private async Task<ImportJobResult> ConfigureAndRunImportApiJobAsync(int workspaceArtifactId, ImportDataTableWrapper dataTable, ImportAPI importApi)
		{
			ImportBulkArtifactJob importJob = importApi.NewNativeDocumentImportJob();
			importJob.Settings.CaseArtifactId = workspaceArtifactId;
			importJob.Settings.OverwriteMode = OverwriteModeEnum.AppendOverlay;
			importJob.Settings.DestinationFolderArtifactID = await Rdos.GetRootFolderInstance(_serviceFactory, workspaceArtifactId).ConfigureAwait(false);
			importJob.Settings.IdentityFieldId = _CONTROL_NUMBER_FIELD_ARTIFACT_ID; // Specify the ArtifactID of the document identifier field, such as a control number.
			importJob.SourceData.SourceData = dataTable.DataReader;

			// Extracted text fields
			if (dataTable.ExtractedText)
			{
				importJob.Settings.ExtractedTextFieldContainsFilePath = true;
				importJob.Settings.ExtractedTextEncoding = Encoding.UTF8;
			}
			else
			{
				importJob.Settings.ExtractedTextFieldContainsFilePath = false;
			}

			// Indicates file path for the native file.
			if (dataTable.Natives)
			{
				importJob.Settings.NativeFilePathSourceFieldName = ImportDataTableWrapper.NativeFilePath;
				importJob.Settings.NativeFileCopyMode = NativeFileCopyModeEnum.CopyFiles;
				importJob.Settings.FileNameColumn = ImportDataTableWrapper.FileName;
				importJob.Settings.CopyFilesToDocumentRepository = dataTable.Natives;
				importJob.Settings.FolderPathSourceFieldName = ImportDataTableWrapper.FolderPath;
			}
			else
			{
				importJob.Settings.NativeFileCopyMode = NativeFileCopyModeEnum.DoNotImportNativeFiles;

				importJob.Settings.DisableNativeLocationValidation = null;
				importJob.Settings.DisableNativeValidation = null;
				importJob.Settings.CopyFilesToDocumentRepository = false;
			}

			return await ImportJobExecutor.ExecuteAsync(importJob).ConfigureAwait(false);
		}
	}
}
