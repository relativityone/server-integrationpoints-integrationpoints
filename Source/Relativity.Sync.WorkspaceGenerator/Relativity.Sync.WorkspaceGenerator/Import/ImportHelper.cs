using System;
using System.Data;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;
using Relativity.Sync.WorkspaceGenerator.RelativityServices;
using Relativity.Sync.WorkspaceGenerator.Settings;

namespace Relativity.Sync.WorkspaceGenerator.Import
{
	internal sealed class ImportHelper
	{
		private const int _CONTROL_NUMBER_FIELD_ARTIFACT_ID = 1003667;

		private readonly WorkspaceService _workspaceService;
		private readonly GeneratorSettings _settings;

		public ImportHelper(WorkspaceService workspaceService, GeneratorSettings settings)
		{
			_workspaceService = workspaceService;
			_settings = settings;
		}

		public async Task<ImportJobResult> ImportDataAsync(int workspaceArtifactId, IDataReader dataReader)
		{
			kCura.WinEDDS.Config.ConfigSettings[nameof(kCura.WinEDDS.Config.TapiForceHttpClient)] = true.ToString(CultureInfo.InvariantCulture);
			kCura.WinEDDS.Config.ConfigSettings[nameof(kCura.WinEDDS.Config.TapiForceBcpHttpClient)] = true.ToString(CultureInfo.InvariantCulture);

			Console.WriteLine("Creating ImportAPI client");
			var importApi =
				new ImportAPI(
					_settings.RelativityUserName,
					_settings.RelativityPassword,
					_settings.RelativityWebApiUri.ToString());

			Console.WriteLine("Importing documents");
			ImportJobResult result = await ConfigureAndRunImportApiJobAsync(workspaceArtifactId, dataReader, importApi).ConfigureAwait(false);

			return result;
		}

		private async Task<ImportJobResult> ConfigureAndRunImportApiJobAsync(int workspaceArtifactId, IDataReader dataReader, ImportAPI importApi)
		{
			ImportBulkArtifactJob importJob = importApi.NewNativeDocumentImportJob();
			importJob.Settings.CaseArtifactId = workspaceArtifactId;
			importJob.Settings.OverwriteMode = OverwriteModeEnum.AppendOverlay;
			importJob.Settings.DestinationFolderArtifactID = await _workspaceService.GetRootFolderArtifactIDAsync(workspaceArtifactId).ConfigureAwait(false);
			importJob.Settings.IdentityFieldId = _CONTROL_NUMBER_FIELD_ARTIFACT_ID; // Specify the ArtifactID of the document identifier field, such as a control number.
			importJob.SourceData.SourceData = dataReader;

			// Extracted text fields
			if (_settings.GenerateExtractedText)
			{
				importJob.Settings.ExtractedTextFieldContainsFilePath = true;
				importJob.Settings.ExtractedTextEncoding = Encoding.UTF8;
			}
			else
			{
				importJob.Settings.ExtractedTextFieldContainsFilePath = false;
			}

			// Indicates file path for the native file.
			if (_settings.GenerateNatives)
			{
				importJob.Settings.CopyFilesToDocumentRepository = true;
				importJob.Settings.NativeFileCopyMode = NativeFileCopyModeEnum.CopyFiles;
				importJob.Settings.NativeFilePathSourceFieldName = ColumnNames.NativeFilePath;
				importJob.Settings.FileNameColumn = ColumnNames.FileName;
			}
			else
			{
				importJob.Settings.NativeFileCopyMode = NativeFileCopyModeEnum.DoNotImportNativeFiles;
			}

			return await ImportJobExecutor.ExecuteAsync(importJob).ConfigureAwait(false);
		}
	}

}