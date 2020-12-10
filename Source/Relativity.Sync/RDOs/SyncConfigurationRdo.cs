using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.RDOs
{
	public class SyncConfigurationRdo
	{
		public int RdoArtifactTypeId { get; set; }
		public string DataSourceType { get; set; }
		public int DataSourceArtifactId { get; set; }

		public int DestinationWorkspaceArtifactId { get; set; }
		public int DataDestinationArtifactId { get; set; }
		public string DataDestinationType { get; set; }
		public string DestinationFolderStructureBehavior { get; set; }
		public string FolderPathSourceFieldName { get; set; }
		public bool CreateSavedSearchInDestination { get; set; }

		public string ImportOverwriteMode { get; set; }
		public string FieldOverlayBehavior { get; set; }

		public string FieldsMapping { get; set; }
		public bool MoveExistingDocuments { get; set; }
		public string NativesBehavior { get; set; }

		public bool ImageImport { get; set; }
		public bool IncludeOriginalImages { get; set; }
		public string ProductionImagePrecedence { get; set; }
		public string ImageFileCopyMode { get; set; }

		public string EmailNotificationRecipients { get; set; }

		public RelativityObject JobHistoryToRetry { get; set; }

		#region GUIDs
		public static readonly Guid SyncConfigurationGuid = new Guid("3BE3DE56-839F-4F0E-8446-E1691ED5FD57");

		public static readonly Guid CreateSavedSearchInDestinationGuid = new Guid("BFAB4AF6-4704-4A12-A8CA-C96A1FBCB77D");
		public static readonly Guid DataDestinationArtifactIdGuid = new Guid("0E9D7B8E-4643-41CC-9B07-3A66C98248A1");
		public static readonly Guid DataDestinationTypeGuid = new Guid("86D9A34A-B394-41CF-BFF4-BD4FF49A932D");
		public static readonly Guid DataSourceArtifactIdGuid = new Guid("6D8631F9-0EA1-4EB9-B7B2-C552F43959D0");
		public static readonly Guid DataSourceTypeGuid = new Guid("A00E6BC1-CA1C-48D9-9712-629A63061F0D");
		public static readonly Guid DestinationFolderStructureBehaviorGuid = new Guid("A1593105-BD99-4A15-A51A-3AA8D4195908");
		public static readonly Guid DestinationWorkspaceArtifactIdGuid = new Guid("15B88438-6CF7-47AB-B630-424633159C69");
		public static readonly Guid EmailNotificationRecipientsGuid = new Guid("4F03914D-9E86-4B72-B75C-EE48FEEBB583");
		public static readonly Guid FieldMappingsGuid = new Guid("E3CB5C64-C726-47F8-9CB0-1391C5911628");
		public static readonly Guid FieldOverlayBehaviorGuid = new Guid("34ECB263-1370-4D6C-AC11-558447504EC4");
		public static readonly Guid FolderPathSourceFieldNameGuid = new Guid("66A37443-EF92-47ED-BEEA-392464C853D3");
		public static readonly Guid ImportOverwriteModeGuid = new Guid("1914D2A3-A1FF-480B-81DC-7A2AA563047A");
		public static readonly Guid MoveExistingDocumentsGuid = new Guid("26F9BF88-420D-4EFF-914B-C47BA36E10BF");
		public static readonly Guid NativesBehaviorGuid = new Guid("D18F0199-7096-4B0C-AB37-4C9A3EA1D3D2");
		public static readonly Guid RdoArtifactTypeIdGuid = new Guid("4DF15F2B-E566-43CE-830D-671BD0786737");

		public static readonly Guid JobHistoryToRetryGuid = new Guid("d7d0ddb9-d383-4578-8d7b-6cbdd9e71549");

		public static readonly Guid ImageImportGuid = new Guid("b282bbe4-7b32-41d1-bb50-960a0e483bb5");
		public static readonly Guid IncludeOriginalImagesGuid = new Guid("f2cad5c5-63d5-49fc-bd47-885661ef1d8b");
		public static readonly Guid ProductionImagePrecedenceGuid = new Guid("421cf05e-bab4-4455-a9ca-fa83d686b5ed");
		public static readonly Guid ImageFileCopyModeGuid = new Guid("bd5dc6d2-faa2-4312-8dc0-4d1b6945dfe1");
		#endregion
	}
}
