

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.Relativity.Client.DTOs;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;
using Relativity.Services;
using Status = kCura.Relativity.DataReaderClient.Status;
using TextCondition = kCura.Relativity.Client.TextCondition;
using TextConditionEnum = kCura.Relativity.Client.TextConditionEnum;
using Workspace = kCura.Relativity.Client.DTOs.Workspace;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers
{
	internal class WorkspaceService
	{
		private IntegrationPoint.Tests.Core.Helper _helper;

		public WorkspaceService(IntegrationPoint.Tests.Core.Helper helper)
		{
			_helper = helper;
		}

		internal void ImportDocument(int workspaceArtifactID, IDataReader dataReader)
		{
			// Predifined Control Number artifact id
			Int32 identifyFieldArtifactID = 1003667;

			String relativityUserName = _helper.SharedVariables.RelativityUserName;
			String relativityPassword = _helper.SharedVariables.RelativityPassword;

			String relativityWebAPIUrl = "http://localhost/Relativitywebapi/";

			ImportAPI iapi = new ImportAPI(relativityUserName, relativityPassword, relativityWebAPIUrl);

			var importJob = iapi.NewNativeDocumentImportJob();

			importJob.OnMessage += ImportJobOnMessage;
			importJob.OnComplete += ImportJobOnComplete;
			importJob.OnFatalException += ImportJobOnFatalException;
			importJob.Settings.CaseArtifactId = workspaceArtifactID;
			importJob.Settings.ExtractedTextFieldContainsFilePath = false;

			// Indicates file path for the native file.
			importJob.Settings.NativeFilePathSourceFieldName = "Native File";

			// Indicates the column containing the ID of the parent document.
			//importJob.Settings.ParentObjectIdSourceFieldName = "Parent Document ID";

			// Indicates the column containing the ID of the Data Grid records already created for the documents.
			//importJob.Settings.DataGridIDColumnName = "Data Grid ID";

			// The name of the document identifier column must match the name of the document identifier field
			// in the workspace.
			//importJob.Settings.SelectedIdentifierFieldName = "Doc ID Beg";
			importJob.Settings.NativeFileCopyMode = NativeFileCopyModeEnum.CopyFiles;
			importJob.Settings.OverwriteMode = OverwriteModeEnum.AppendOverlay;
			importJob.Settings.FileNameColumn = "File Name";
			importJob.Settings.CopyFilesToDocumentRepository = true;

			// Specify the ArtifactID of the document identifier field, such as a control number.
			importJob.Settings.IdentityFieldId = identifyFieldArtifactID;

			importJob.SourceData.SourceData = dataReader;

			Console.WriteLine("Executing import...");

			importJob.Execute();
		}

		internal DataTable GetDocumentDataTable()
		{
			DataTable table = new DataTable();

			// The document identifer column name must match the field name in the workspace.
			table.Columns.Add("Control Number", typeof (string));
			table.Columns.Add("File Name", typeof(string));
			table.Columns.Add("Native File", typeof(string));
			table.Rows.Add("SBECK_0048462", "SBECK_0048462.docx", "E:\\Datasets\\Import\\AdminTrainingSampleData\\Sample01\\NATIVES\\NATIVES001\\SBECK_0048462.docx");
			table.Rows.Add("SBECK_0048461", "SBECK_0048461.docx", "E:\\Datasets\\Import\\AdminTrainingSampleData\\Sample01\\NATIVES\\NATIVES001\\SBECK_0048461.docx");

			return table;
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
	}
}
