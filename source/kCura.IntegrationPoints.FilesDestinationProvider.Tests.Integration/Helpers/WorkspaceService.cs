using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using kCura.IntegrationPoint.Tests.Core;
using kCura.Relativity.Client;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;
using Status = kCura.Relativity.DataReaderClient.Status;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers
{
	internal class WorkspaceService
	{
		#region Fileds

		private readonly Helper _helper;
		private readonly ConfigSettings _configSettings;

		private const string TemplateWorkspaceName = "kCura Starter Template";
		private const int ControlNumberFieldArtifactId = 1003667;

		#endregion //Fileds

		#region Constructors

		public WorkspaceService(Helper helper, ConfigSettings configSettings)
		{
			_helper = helper;
			_configSettings = configSettings;
		}

		#endregion //Constructors

		#region Methods

		internal int CreateWorkspace(string name)
		{
			return _helper.Workspace.CreateWorkspace(name, TemplateWorkspaceName);
		}

		internal void DeleteWorkspace(int artifactId)
		{
			using (IRSAPIClient rsApiClient = _helper.Rsapi.CreateRsapiClient())
			{
				rsApiClient.Repositories.Workspace.DeleteSingle(artifactId);
			}
		}

		internal int GetSavedSearchIdBy(string name, int workspaceId)
		{
			using (IRSAPIClient rsApiClient = _helper.Rsapi.CreateRsapiClient())
			{
				var query = new Query
				{
					ArtifactTypeID = (int)ArtifactType.Search,
					Condition = new TextCondition("Name", TextConditionEnum.Like, name),
				};

				rsApiClient.APIOptions.WorkspaceID = workspaceId;
				QueryResult result = rsApiClient.Query(rsApiClient.APIOptions, query);

				return result.QueryArtifacts[0].ArtifactID;
			}
		}

		internal IEnumerable<int> GetFieldIdsBy(List<string> listName, int workspaceId)
		{
			ImportAPI importApi = new ImportAPI(_helper.SharedVariables.RelativityUserName,
				_helper.SharedVariables.RelativityPassword);

			IEnumerable<Relativity.ImportAPI.Data.Field> fields = importApi.GetWorkspaceFields(workspaceId, (int)ArtifactType.Document);

			return fields.Where(field => listName.Contains(field.Name)).Select(field => field.ArtifactID);
		}

		internal void ImportData(int workspaceArtifactId, DataTable nativeFilesSourceDataTable, DataTable imageSourceDataTable)
		{
			string relativityUserName = _helper.SharedVariables.RelativityUserName;
			string relativityPassword = _helper.SharedVariables.RelativityPassword;

			ImportAPI iapi = new ImportAPI(relativityUserName, relativityPassword, _configSettings.WebApiUrl);

			ImportNativeFiles(workspaceArtifactId, nativeFilesSourceDataTable.CreateDataReader(), iapi, ControlNumberFieldArtifactId);
			ImportImagesAndExtractedText(workspaceArtifactId, imageSourceDataTable, iapi, ControlNumberFieldArtifactId);
		}

		private void ImportImagesAndExtractedText(int workspaceArtifactId, DataTable dataTable, ImportAPI importApi, int identifyFieldArtifactId)
		{
			var importJob = importApi.NewImageImportJob();

			importJob.OnMessage += ImportJobOnMessage;
			importJob.OnComplete += ImportJobOnComplete;
			importJob.OnFatalException += ImportJobOnFatalException;

			importJob.Settings.AutoNumberImages = false;

			importJob.Settings.CaseArtifactId = workspaceArtifactId;
			importJob.Settings.ExtractedTextFieldContainsFilePath = true;
			importJob.Settings.ExtractedTextEncoding = Encoding.UTF8;

			importJob.Settings.DocumentIdentifierField = "Control Number";

			// Indicates filepath for an image.
			importJob.Settings.FileLocationField = "File";
			importJob.Settings.BatesNumberField = "Bates Beg";
			//Indicates that the images must be copied to the document repository
			importJob.Settings.CopyFilesToDocumentRepository = true;

			// Specifies the ArtifactID of a document identifier field, such as a control number.
			importJob.Settings.IdentityFieldId = identifyFieldArtifactId;
			importJob.Settings.OverwriteMode = OverwriteModeEnum.Append;
			importJob.SourceData.SourceData = dataTable;

			Console.WriteLine("Executing native import...");

			importJob.Execute();
		}

		private static void ImportNativeFiles(int workspaceArtifactId, IDataReader dataReader, ImportAPI importApi, int identifyFieldArtifactId)
		{
			var importJob = importApi.NewNativeDocumentImportJob();
			importJob.OnMessage += ImportJobOnMessage;
			importJob.OnComplete += ImportJobOnComplete;
			importJob.OnFatalException += ImportJobOnFatalException;
			importJob.Settings.CaseArtifactId = workspaceArtifactId;
			importJob.Settings.ExtractedTextFieldContainsFilePath = false;

			// Indicates file path for the native file.
			importJob.Settings.NativeFilePathSourceFieldName = "Native File";
			importJob.Settings.NativeFileCopyMode = NativeFileCopyModeEnum.CopyFiles;
			importJob.Settings.OverwriteMode = OverwriteModeEnum.Append;
			importJob.Settings.FileNameColumn = "File Name";
			importJob.Settings.CopyFilesToDocumentRepository = true;

			// Specify the ArtifactID of the document identifier field, such as a control number.
			importJob.Settings.IdentityFieldId = identifyFieldArtifactId;

			importJob.SourceData.SourceData = dataReader;

			Console.WriteLine("Executing import native files...");

			importJob.Execute();
		}

		private static void ImportJobOnFatalException(JobReport jobreport)
		{
			if(jobreport.ErrorRows.Any())
				jobreport.ErrorRows.ToList().ForEach(error => Console.WriteLine(error.Message));
		}

		private static void ImportJobOnComplete(JobReport jobreport)
		{
			if (jobreport.ErrorRows.Any())
				jobreport.ErrorRows.ToList().ForEach(error => Console.WriteLine(error.Message));
		}

		private static void ImportJobOnMessage(Status status)
		{
			Console.WriteLine(status.Message);
		}

		#endregion Methods
	}
}
