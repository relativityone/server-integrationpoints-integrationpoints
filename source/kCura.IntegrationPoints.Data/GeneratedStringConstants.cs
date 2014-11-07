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
		}

	public partial class ObjectTypeGuids
	{
		public const string IntegrationPoints = @"03d4f67e-22c9-488c-bee6-411f05c52e01";
		}

	#region "Field Constants"
	
	public partial class IntegrationPointsFields : BaseFields
	{
		public const string Name = @"Name";
	}

	public partial class IntegrationPointsFieldGuids 
	{
		public const string Name = @"d534f433-dd92-4a53-b12d-bf85472e6d7a";
	}



	#endregion

	#region "Choice Constants"

	#endregion								

	#region "Layouts"

	public partial class IntegrationPointsLayoutGuids
	{
		public const string IntegrationPointsLayout = @"17f2e3ae-bc4c-49db-9c75-e4b0768a32f2";
	}

	public partial class IntegrationPointsLayouts
	{
		public const string IntegrationPointsLayout = @"Integration Points Layout";
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

	#endregion									

}
