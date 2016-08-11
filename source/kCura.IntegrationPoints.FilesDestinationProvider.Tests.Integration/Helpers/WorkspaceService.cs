using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.Relativity.Client;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;
using Relativity.API;
using Relativity.Services.Field;
using Relativity.Services.Search;
using Status = kCura.Relativity.DataReaderClient.Status;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers
{
	internal class WorkspaceService
	{
		#region Fields

		private readonly ConfigSettings _configSettings;

		private const string TemplateWorkspaceName = "kCura Starter Template";
		private const int _ControlNumber_Field_ArtifactId = 1003667;
	    private const string _SAVED_SEARCH_FOLDER = "Testing Folder";
	    private const string _SAVED_SEARCH_NAME = "Testing Saved Search";

        #endregion //Fields

        #region Constructors

        public WorkspaceService(ConfigSettings configSettings)
		{
			_configSettings = configSettings;
		}

		#endregion //Constructors

		#region Methods

		internal int CreateWorkspace(string name)
		{
			return Workspace.CreateWorkspace(name, TemplateWorkspaceName);
		}

		internal void DeleteWorkspace(int artifactId)
		{
			using (IRSAPIClient rsApiClient = Rsapi.CreateRsapiClient(ExecutionIdentity.System))
			{
				rsApiClient.Repositories.Workspace.DeleteSingle(artifactId);
			}
		}

		internal int GetSavedSearchIdBy(string name, int workspaceId)
		{
			using (IRSAPIClient rsApiClient = Rsapi.CreateRsapiClient(ExecutionIdentity.System))
			{
				var query = new Query
				{
					ArtifactTypeID = (int)ArtifactType.Search,
					Condition = new TextCondition("Name", TextConditionEnum.EqualTo, name),
				};

				rsApiClient.APIOptions.WorkspaceID = workspaceId;
				QueryResult result = rsApiClient.Query(rsApiClient.APIOptions, query);

				return result.QueryArtifacts[0].ArtifactID;
			}
		}

	    internal int CreateSavedSearch(FieldEntry[] defaultFields, FieldEntry[] additionalFields, int workspaceId)
	    {
	        var query =
	            defaultFields.Select(x => new FieldRef(x.DisplayName))
	                .Concat(additionalFields.Select(x => new FieldRef(x.DisplayName)));

            SearchContainer folder = new SearchContainer()
            {
                Name = _SAVED_SEARCH_FOLDER,
            };
            int folderArtifactId = SavedSearch.CreateSearchFolder(workspaceId, folder);

            KeywordSearch search = new KeywordSearch()
            {
                Name = _SAVED_SEARCH_NAME,
                ArtifactTypeID = (int)ArtifactType.Document,
                SearchContainer = new SearchContainerRef(folderArtifactId),
                Fields = new List<FieldRef>(query.ToArray())
            };
            return SavedSearch.Create(workspaceId, search);
        }

		internal void ImportData(int workspaceArtifactId, DataTable nativeFilesSourceDataTable, DataTable imageSourceDataTable)
		{
			ImportAPI importApi = new ImportAPI(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword, _configSettings.WebApiUrl);

			ImportNativeFiles(workspaceArtifactId, nativeFilesSourceDataTable.CreateDataReader(), importApi, _ControlNumber_Field_ArtifactId);
			ImportImagesAndExtractedText(workspaceArtifactId, imageSourceDataTable, importApi, _ControlNumber_Field_ArtifactId);
		}

		private static void ImportImagesAndExtractedText(int workspaceArtifactId, DataTable dataTable, ImportAPI importApi, int identifyFieldArtifactId)
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
			importJob.Settings.OverwriteMode = OverwriteModeEnum.Overlay;
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
			if (jobreport.ErrorRows.Any())
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