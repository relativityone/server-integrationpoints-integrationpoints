using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Domain;
using Newtonsoft.Json;
using Relativity.Core;
using Relativity.Core.Service;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public class ImportSettings
	{
		public const string FIELDOVERLAYBEHAVIOR_MERGE = "Merge Values";
		public const string FIELDOVERLAYBEHAVIOR_REPLACE = "Replace Values";
		public const string FIELDOVERLAYBEHAVIOR_DEFAULT = "Use Field Settings";

		private readonly IAuditSpoofTokenGenerator _generator;
		private BaseServiceContext _context;

		private BaseServiceContext Context => _context ?? (_context = ClaimsPrincipal.Current.GetUnversionContext(CaseArtifactId));

		#region "Constructors"

		public ImportSettings() :
			this(new AuditSpoofTokenGenerator(), null)
		{ }

		public ImportSettings(IAuditSpoofTokenGenerator generator, BaseServiceContext context)
		{
			_generator = generator;
			_context = context;
			ImportAuditLevel = ImportAuditLevelEnum.NoSnapshot;
			ExtractedTextFieldContainsFilePath = false;
			MultiValueDelimiter = Constants.MULTI_VALUE_DELIMITER;
			ImportNativeFileCopyMode = ImportNativeFileCopyModeEnum.DoNotImportNativeFiles;
			NestedValueDelimiter = Constants.NESTED_VALUE_DELIMITER;
			ImportOverwriteMode = ImportOverwriteModeEnum.AppendOnly;
		}

		#endregion "Constructors"

		#region "Public Properties"

		[JsonProperty(PropertyName = "artifactTypeID")]
		public int ArtifactTypeId { get; set; }
		public string BulkLoadFileFieldDelimiter { get; set; }
		public int CaseArtifactId { get; set; }
		public bool CopyFilesToDocumentRepository { get; set; }
		public bool CustodianManagerFieldContainsLink { get; set; }
		public int DestinationFolderArtifactId { get; set; }
		public bool DisableControlNumberCompatibilityMode { get; set; }
		public bool? DisableExtractedTextEncodingCheck { get; set; }
		public bool DisableExtractedTextFileLocationValidation { get; set; }
		public bool? DisableNativeLocationValidation { get; set; }
		public bool? DisableNativeValidation { get; set; }
		public string DestinationProviderType { get; set; }
		public bool DisableUserSecurityCheck { get; set; }

		[JsonIgnore]
		public Encoding ExtractedTextEncoding => Encoding.GetEncoding(ExtractedTextFileEncoding);

		public bool ExtractedTextFieldContainsFilePath { get; set; }
		public String ExtractedTextFileEncoding { get; set; }

        /// <summary>
        /// Indicates user preferences of field overlay behavior.
        /// </summary>
        /// <remarks>
        /// possible values : 'Merge Values', 'Replace Values', 'Use Field Settings'
        /// value is assigned in step-mapFields.js
        /// </remarks>
        public string FieldOverlayBehavior { get; set; }

		public string FileNameColumn { get; set; }
		public string FileSizeColumn { get; set; }
		public bool FileSizeMapped { get; set; }
		public string FolderPathSourceFieldName { get; set; }
		public int IdentityFieldId { get; set; }
		public ImportAuditLevelEnum ImportAuditLevel { get; set; }

		/// <summary>
		/// Indicates user preferences of importing native files.
		/// </summary>
		public bool ImportNativeFile { get; set; }

		public ImportNativeFileCopyModeEnum ImportNativeFileCopyMode { get; set; }

		public ImportOverlayBehaviorEnum ImportOverlayBehavior
		{
			get
			{
				if (String.IsNullOrEmpty(FieldOverlayBehavior) || FieldOverlayBehavior == FIELDOVERLAYBEHAVIOR_DEFAULT)
				{
					return ImportOverlayBehaviorEnum.UseRelativityDefaults;
				}
				else if (FieldOverlayBehavior == FIELDOVERLAYBEHAVIOR_MERGE)
				{
					return ImportOverlayBehaviorEnum.MergeAll;
				}
				else if (FieldOverlayBehavior == FIELDOVERLAYBEHAVIOR_REPLACE)
				{
					return ImportOverlayBehaviorEnum.ReplaceAll;
				}
				else
				{
					throw new Exception($"Unable to determine Import Overlay Behavior : {FieldOverlayBehavior}");
				}
			}
		}

		public ImportOverwriteModeEnum ImportOverwriteMode { get; set; }
        public string LongTextColumnThatContainsPathToFullText { get; set; }
        public int MaximumErrorCount { get; set; }
		public char MultiValueDelimiter { get; set; }
		public string NativeFilePathSourceFieldName { get; set; }
		public char NestedValueDelimiter { get; set; }
		public IList<int> ObjectFieldIdListContainsArtifactId { get; set; }
		public string OIFileIdColumnName { get; set; }
		public bool OIFileIdMapped { get; set; }
		public string OIFileTypeColumnName { get; set; }
		public int OnBehalfOfUserId { get; set; }
		public string ParentObjectIdSourceFieldName { get; set; }
		public string Provider { get; set; }
		public string RelativityPassword { get; set; }

		public string RelativityUsername { get; set; }

		public bool SendEmailOnLoadCompletion { get; set; }

		public string SelectedCaseFileRepoPath { get; set; }

		public int StartRecordNumber { get; set; }

		public string WebServiceURL { get; set; }

		[JsonIgnore]
		internal string OnBehalfOfUserToken
		{
			get
			{
				if (OnBehalfOfUserId <= 0)
				{
					return String.Empty;
				}
				return _generator.Create(Context, OnBehalfOfUserId);
			}
		}

		public bool AutoNumberImages { get; set; }
		public bool ProductionImport { get; set; }
		public bool ImageImport { get; set; }
		public int ProductionArtifactId { get; set; }

		#endregion "Public Properties"

		#region "Internal Properties"

		[JsonIgnore]
		internal kCura.Relativity.DataReaderClient.NativeFileCopyModeEnum NativeFileCopyMode
		{
			get { return (kCura.Relativity.DataReaderClient.NativeFileCopyModeEnum)this.ImportNativeFileCopyMode; }
		}

		[JsonIgnore]
		internal kCura.EDDS.WebAPI.BulkImportManagerBase.OverlayBehavior OverlayBehavior
		{
			get { return (kCura.EDDS.WebAPI.BulkImportManagerBase.OverlayBehavior)this.ImportOverlayBehavior; }
		}

		[JsonIgnore]
		internal kCura.Relativity.DataReaderClient.OverwriteModeEnum OverwriteMode
		{
			get { return (kCura.Relativity.DataReaderClient.OverwriteModeEnum)this.ImportOverwriteMode; }

			set { this.ImportOverwriteMode = (ImportOverwriteModeEnum)value; }
		}

		[JsonIgnore]
		internal kCura.EDDS.WebAPI.BulkImportManagerBase.ImportAuditLevel AuditLevel
		{
			get { return (kCura.EDDS.WebAPI.BulkImportManagerBase.ImportAuditLevel)this.ImportAuditLevel; }
		}
		#endregion "Internal Properties"
	}
}