using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.EDDS.WebAPI.BulkImportManagerBase;
using kCura.Relativity.DataReaderClient;

namespace kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI
{
	public class ImportSettings
	{

		#region "Constructors"
		public ImportSettings()
		{
			AuditLevel = ImportAuditLevel.FullAudit;
			ExtractedTextFieldContainsFilePath = false;
			MultiValueDelimiter = Convert.ToChar(";");
			NativeFileCopyMode = NativeFileCopyModeEnum.DoNotImportNativeFiles;
			NestedValueDelimiter = Convert.ToChar("\\");
			OverwriteMode = OverwriteModeEnum.Append;
			OverlayBehavior = OverlayBehavior.UseRelativityDefaults;
		}
		#endregion

		#region "Properties"
		public int ArtifactTypeId { get; set; }
		public string BulkLoadFileFieldDelimiter { get; set; }
		public bool DisableControlNumberCompatibilityMode { get; set; }
		public bool DisableExtractedTextFileLocationValidation { get; set; }
		public bool? DisableNativeLocationValidation { get; set; }
		public bool? DisableNativeValidation { get; set; }
		public string FolderPathSourceFieldName { get; set; }
		public char MultiValueDelimiter { get; set; }
		public string NativeFilePathSourceFieldName { get; set; }
		public char NestedValueDelimiter { get; set; }
		internal string OnBehalfOfUserToken { get; set; }
		public bool OIFileIdMapped { get; set; }
		public string OIFileIdColumnName { get; set; }
		public string OIFileTypeColumnName { get; set; }
		public bool FileSizeMapped { get; set; }
		public string FileSizeColumn { get; set; }
		public ImportAuditLevel AuditLevel { get; set; }
		public int CaseArtifactId { get; set; }
		public bool CopyFilesToDocumentRepository { get; set; }
		public int DestinationFolderArtifactID { get; set; }
		public bool? DisableExtractedTextEncodingCheck { get; set; }
		public bool DisableUserSecurityCheck { get; set; }
		public Encoding ExtractedTextEncoding { get; set; }
		public bool ExtractedTextFieldContainsFilePath { get; set; }
		public int IdentityFieldId { get; set; }
		public int MaximumErrorCount { get; set; }
		public NativeFileCopyModeEnum NativeFileCopyMode { get; set; }
		public OverwriteModeEnum OverwriteMode { get; set; }
		public string ParentObjectIdSourceFieldName { get; set; }
		public string RelativityPassword { get; set; }
		public string RelativityUsername { get; set; }
		public bool SendEmailOnLoadCompletion { get; set; }
		public string WebServiceURL { get; set; }
		public int StartRecordNumber { get; set; }
		public IList<int> ObjectFieldIdListContainsArtifactId { get; set; }
		public OverlayBehavior OverlayBehavior { get; set; }

		#endregion

	}
}
