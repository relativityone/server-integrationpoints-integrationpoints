using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public class ImportSettings
	{

		#region "Constructors"
		public ImportSettings()
		{
			ImportAuditLevel = ImportAuditLevelEnum.FullAudit;
			ExtractedTextFieldContainsFilePath = false;
			MultiValueDelimiter = Convert.ToChar(";");
			ImportNativeFileCopyMode = ImportNativeFileCopyModeEnum.DoNotImportNativeFiles;
			NestedValueDelimiter = Convert.ToChar("\\");
			ImportOverwriteMode = ImportOverwriteModeEnum.Append;
			ImportOverlayBehavior = ImportOverlayBehaviorEnum.UseRelativityDefaults;
		}
		#endregion

		#region "Public Properties"
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
		public ImportAuditLevelEnum ImportAuditLevel { get; set; }
		public int CaseArtifactId { get; set; }
		public bool CopyFilesToDocumentRepository { get; set; }
		public int DestinationFolderArtifactID { get; set; }
		public bool? DisableExtractedTextEncodingCheck { get; set; }
		public bool DisableUserSecurityCheck { get; set; }
		public Encoding ExtractedTextEncoding { get; set; }
		public bool ExtractedTextFieldContainsFilePath { get; set; }
		public int IdentityFieldId { get; set; }
		public int MaximumErrorCount { get; set; }
		public ImportNativeFileCopyModeEnum ImportNativeFileCopyMode { get; set; }
		public ImportOverwriteModeEnum ImportOverwriteMode { get; set; }
		public string ParentObjectIdSourceFieldName { get; set; }
		public string RelativityPassword { get; set; }
		public string RelativityUsername { get; set; }
		public bool SendEmailOnLoadCompletion { get; set; }
		public string WebServiceURL { get; set; }
		public int StartRecordNumber { get; set; }
		public IList<int> ObjectFieldIdListContainsArtifactId { get; set; }
		public ImportOverlayBehaviorEnum ImportOverlayBehavior { get; set; }
		public bool CustodianManagerFieldContainsLink{ get; set; }
		#endregion

		#region "Internal Properties"
		[JsonIgnore]
		internal kCura.EDDS.WebAPI.BulkImportManagerBase.ImportAuditLevel AuditLevel
		{
			get { return (kCura.EDDS.WebAPI.BulkImportManagerBase.ImportAuditLevel)this.ImportAuditLevel; }
		}
		[JsonIgnore]
		internal kCura.Relativity.DataReaderClient.NativeFileCopyModeEnum NativeFileCopyMode
		{
			get { return (kCura.Relativity.DataReaderClient.NativeFileCopyModeEnum)this.ImportNativeFileCopyMode; }
		}
		[JsonIgnore]
		internal kCura.Relativity.DataReaderClient.OverwriteModeEnum OverwriteMode
		{
			get { return (kCura.Relativity.DataReaderClient.OverwriteModeEnum)this.ImportOverwriteMode; }
		}
		[JsonIgnore]
		internal kCura.EDDS.WebAPI.BulkImportManagerBase.OverlayBehavior OverlayBehavior
		{
			get { return (kCura.EDDS.WebAPI.BulkImportManagerBase.OverlayBehavior)this.ImportOverlayBehavior; }
		}
		#endregion

	}
}
