using kCura.Relativity.Client;
namespace kCura.IntegrationPoints.Data
{
 
	public class BaseFields
	{
		public const string SystemCreatedOn = "System Created On";
		public const string SystemCreatedBy = "System Created By";
		public const string LastModifiedOn = "Last Modified On";
		public const string LastModifiedBy = "Last Modified By";
	}

	public class FieldTypes
	{
		public const string SingleObject = "Single Object";
		public const string MultipleObject = "Multiple Object";
		public const string FixedLengthText = "Fixed Length Text";
		public const string SingleChoice = "Single Choice";
		public const string MultipleChoice = "MultipleChoice";
		public const string LongText = "Long Text";
		public const string User = "User";
		public const string YesNo = "Yes/No";
		public const string WholeNumber = "Whole Number";
		public const string Currency = "Currency";
		public const string Decimal = "Decimal";
		public const string @Date = "Date";
		public const string File = "File";
	}

	public partial class ObjectTypes
	{
		public const string Workspace = "Workspace";
		public const string Folder = "Folder";
		public const string IntegrationPoints = @"Integration Points";
		public const string MappedFields = @"Mapped Fields";
		}

	public partial class ObjectTypeGuids
	{
		public const string IntegrationPoints = @"03d4f67e-22c9-488c-bee6-411f05c52e01";
		public const string MappedFields = @"455e626a-17b1-42ec-9a75-ed81bedbf9c8";
		}

	#region "Field Constants"
	
	public partial class IntegrationPointsFields : BaseFields
	{
		public const string ConnectionPath = @"Connection Path";
		public const string FilterString = @"Filter String";
		public const string Authentication = @"Authentication";
		public const string UserName = @"UserName";
		public const string Password = @"Password";
		public const string OverwriteFieldsOnImport = @"Overwrite Fields on Import";
		public const string NextScheduledRuntime = @"Next Scheduled Runtime";
		public const string LastRuntime = @"Last Runtime";
		public const string Name = @"Name";
	}

	public partial class IntegrationPointsFieldGuids 
	{
		public const string ConnectionPath = @"4b4fcf54-8044-4f3b-bf66-92b090d96297";
		public const string FilterString = @"529e2dd7-7ace-4134-8b24-9f667976a4f0";
		public const string Authentication = @"b2f87d48-74e0-4648-b62e-b441769e3446";
		public const string UserName = @"a0cd9730-e766-4b40-b028-57189328f052";
		public const string Password = @"da248e86-d7b1-42b7-9a78-2b28e21610d0";
		public const string OverwriteFieldsOnImport = @"d5624e1c-49ce-48b1-9402-3d844adf4a90";
		public const string NextScheduledRuntime = @"5b1c9986-f166-40e4-a0dd-a56f185ff30b";
		public const string LastRuntime = @"90d58af1-f79f-40ae-85fc-7e42f84dbcc1";
		public const string Name = @"d534f433-dd92-4a53-b12d-bf85472e6d7a";
	}



	public partial class MappedFieldsFields : BaseFields
	{
		public const string IntegrationPoints = @"IntegrationPoints";
		public const string WorkspaceField = @"Workspace Field";
		public const string SourceField = @"Source Field";
		public const string Name = @"Name";
	}

	public partial class MappedFieldsFieldGuids 
	{
		public const string IntegrationPoints = @"2cf79fcf-619a-49f4-9486-c192e0dd7949";
		public const string WorkspaceField = @"3555568b-47eb-4218-b45d-af73c02495d2";
		public const string SourceField = @"6216686a-df54-4704-a020-25362d62084c";
		public const string Name = @"41277095-a69e-4df5-8a5e-56658606c522";
	}



	#endregion

	#region "Choice Constants"

	public partial class AuthenticationChoices
	{
		public static Choice IntegrationPointsNone = new Choice(0, @"None");
		public static Choice IntegrationPointsAnonymous = new Choice(0, @"Anonymous");
		public static Choice IntegrationPointsFastBind = new Choice(0, @"FastBind");
	}

	#endregion								

	#region "Layouts"

	public partial class IntegrationPointsLayoutGuids
	{
		public const string IntegrationPointsLayout = @"d8bf50c1-ace1-488b-8781-54133a5794be";
	}

	public partial class IntegrationPointsLayouts
	{
		public const string IntegrationPointsLayout = @"Integration Points Layout";
	}

	public partial class MappedFieldsLayoutGuids
	{
		public const string MappedFieldsLayout = @"9c9625b7-a831-46cd-82e9-2bccdb5e74b5";
	}

	public partial class MappedFieldsLayouts
	{
		public const string MappedFieldsLayout = @"Mapped Fields Layout";
	}

	#endregion
	
	
	#region "Tabs"

	public partial class IntegrationPointsTabGuids
	{
		public const string IntegrationPoints = @"136c14c4-daa7-4542-abed-8d4e9b36a5dd";
	}

	public partial class IntegrationPointsTabs
	{
		public const string IntegrationPoints = @"Integration Points";
	}

	#endregion
	
	#region "Views"

	public partial class IntegrationPointsViewGuids
	{
		public const string AllIntegrationPointss = @"181bf82a-e0dc-4a95-955a-0630bccb6afa";
	}

	public partial class IntegrationPointsViews
	{
		public const string AllIntegrationPointss = @"All Integration Pointss";
	}

	public partial class MappedFieldsViewGuids
	{
		public const string AllMappedFieldss = @"ca6daca1-6399-4d71-8717-d77d12bbf067";
	}

	public partial class MappedFieldsViews
	{
		public const string AllMappedFieldss = @"All Mapped Fieldss";
	}

	#endregion									

}
