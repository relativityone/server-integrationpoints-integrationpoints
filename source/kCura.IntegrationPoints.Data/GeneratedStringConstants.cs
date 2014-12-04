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
		public const string IntegrationPoint = @"Integration Point";
		public const string SourceProvider = @"Source Provider";
		}

	public partial class ObjectTypeGuids
	{
		public const string IntegrationPoint = @"03d4f67e-22c9-488c-bee6-411f05c52e01";
		public const string SourceProvider = @"5be4a1f7-87a8-4cbe-a53f-5027d4f70b80";
		}

	#region "Field Constants"
	
	public partial class IntegrationPointFields : BaseFields
	{
		public const string NextScheduledRuntime = @"Next Scheduled Runtime";
		public const string LastRuntime = @"Last Runtime";
		public const string FieldMappings = @"Field Mappings";
		public const string OverwriteFields = @"Overwrite Fields";
		public const string EnableScheduler = @"Enable Scheduler";
		public const string Frequency = @"Frequency";
		public const string Reoccur = @"Reoccur";
		public const string SendOn = @"Send On";
		public const string StartDate = @"Start Date";
		public const string EndDate = @"End Date";
		public const string ScheduledTime = @"Scheduled Time";
		public const string SourceConfiguration = @"Source Configuration";
		public const string DestinationConfiguration = @"Destination Configuration";
		public const string SourceProvider = @"Source Provider";
		public const string Name = @"Name";
	}

	public partial class IntegrationPointFieldGuids 
	{
		public const string NextScheduledRuntime = @"5b1c9986-f166-40e4-a0dd-a56f185ff30b";
		public const string LastRuntime = @"90d58af1-f79f-40ae-85fc-7e42f84dbcc1";
		public const string FieldMappings = @"1b065787-a6e4-4d70-a7ed-f49d770f0bc7";
		public const string OverwriteFields = @"0c0bbc57-b88c-4b3a-9250-7beb0252adbb";
		public const string EnableScheduler = @"bcdafc41-311e-4b66-8084-4a8e0f56ca00";
		public const string Frequency = @"a2c2c3c5-a350-4617-a3e9-ddd284bed868";
		public const string Reoccur = @"bc50bfc6-8ddf-4476-ad12-d99140a8f6dd";
		public const string SendOn = @"d3b03a4d-9e80-492f-bdb7-ecc5f0227bde";
		public const string StartDate = @"05449a69-1923-4aae-936c-63d42ee3248d";
		public const string EndDate = @"8d904115-d503-4a27-98e9-98d442f5ef37";
		public const string ScheduledTime = @"6a38caa0-c3fc-4d66-b915-aaf30d41399b";
		public const string SourceConfiguration = @"b5000e91-82bd-475a-86e9-32fefc04f4b8";
		public const string DestinationConfiguration = @"b1323ca7-34e5-4e6b-8ff1-e8d3b1a5fd0a";
		public const string SourceProvider = @"dc902551-2c9c-4f41-a917-41f4a3ef7409";
		public const string Name = @"d534f433-dd92-4a53-b12d-bf85472e6d7a";
	}



	public partial class SourceProviderFields : BaseFields
	{
		public const string Identifier = @"Identifier";
		public const string Name = @"Name";
	}

	public partial class SourceProviderFieldGuids 
	{
		public const string Identifier = @"d0ecc6c9-472c-4296-83e1-0906f0c0fbb9";
		public const string Name = @"9073997b-319e-482f-92fe-67e0b5860c1b";
	}



	#endregion

	#region "Choice Constants"

	public partial class OverwriteFieldsChoices
	{
		public static Choice IntegrationPointAppend = new Choice(0, @"Append");
		public static Choice IntegrationPointAppendAndOverlay = new Choice(0, @"Append and overlay");
	}

	public partial class FrequencyChoices
	{
		public static Choice IntegrationPointDaily = new Choice(0, @"Daily");
		public static Choice IntegrationPointWeekly = new Choice(0, @"Weekly");
		public static Choice IntegrationPointMonthly = new Choice(0, @"Monthly");
	}

	#endregion								

	#region "Layouts"

	public partial class IntegrationPointLayoutGuids
	{
		public const string IntegrationPointsLayout = @"d8bf50c1-ace1-488b-8781-54133a5794be";
	}

	public partial class IntegrationPointLayouts
	{
		public const string IntegrationPointsLayout = @"Integration Points Layout";
	}

	public partial class SourceProviderLayoutGuids
	{
		public const string SourceProviderLayout = @"6d2ecb5d-ec2d-4b4b-b631-47fada8af8d4";
	}

	public partial class SourceProviderLayouts
	{
		public const string SourceProviderLayout = @"Source Provider Layout";
	}

	#endregion
	
	
	#region "Tabs"

	public partial class IntegrationPointTabGuids
	{
		public const string IntegrationPoints = @"136c14c4-daa7-4542-abed-8d4e9b36a5dd";
	}

	public partial class IntegrationPointTabs
	{
		public const string IntegrationPoints = @"Integration Points";
	}

	public partial class SourceProviderTabGuids
	{
		public const string SourceProvider = @"72b6d3c7-1827-4b52-b16d-fdfa1b8fc898";
	}

	public partial class SourceProviderTabs
	{
		public const string SourceProvider = @"Source Provider";
	}

	#endregion
	
	#region "Views"

	public partial class IntegrationPointViewGuids
	{
		public const string AllIntegrationPointss = @"181bf82a-e0dc-4a95-955a-0630bccb6afa";
	}

	public partial class IntegrationPointViews
	{
		public const string AllIntegrationPointss = @"All Integration Pointss";
	}

	public partial class SourceProviderViewGuids
	{
		public const string AllSourceProviders = @"f4e2c372-da19-4bb2-9c46-a1d6fa037136";
	}

	public partial class SourceProviderViews
	{
		public const string AllSourceProviders = @"All Source Providers";
	}

	#endregion									

}
