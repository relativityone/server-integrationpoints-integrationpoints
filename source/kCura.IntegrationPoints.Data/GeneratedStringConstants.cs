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
		public const string DestinationProvider = @"DestinationProvider";
		}

	public partial class ObjectTypeGuids
	{
		public const string IntegrationPoint = @"03d4f67e-22c9-488c-bee6-411f05c52e01";
		public const string SourceProvider = @"5be4a1f7-87a8-4cbe-a53f-5027d4f70b80";
		public const string DestinationProvider = @"d014f00d-f2c0-4e7a-b335-84fcb6eae980";
		}

	#region "Field Constants"
	
	public partial class IntegrationPointFields : BaseFields
	{
		public const string NextScheduledRuntime = @"Next Scheduled Runtime";
		public const string LastRuntime = @"Last Runtime";
		public const string FieldMappings = @"Field Mappings";
		public const string EnableScheduler = @"Enable Scheduler";
		public const string SourceConfiguration = @"Source Configuration";
		public const string DestinationConfiguration = @"Destination Configuration";
		public const string SourceProvider = @"Source Provider";
		public const string ScheduleRule = @"Schedule Rule";
		public const string OverwriteFields = @"Overwrite Fields";
		public const string DestinationProvider = @"Destination Provider";
		public const string Name = @"Name";
	}

	public partial class IntegrationPointFieldGuids 
	{
		public const string NextScheduledRuntime = @"5b1c9986-f166-40e4-a0dd-a56f185ff30b";
		public const string LastRuntime = @"90d58af1-f79f-40ae-85fc-7e42f84dbcc1";
		public const string FieldMappings = @"1b065787-a6e4-4d70-a7ed-f49d770f0bc7";
		public const string EnableScheduler = @"bcdafc41-311e-4b66-8084-4a8e0f56ca00";
		public const string SourceConfiguration = @"b5000e91-82bd-475a-86e9-32fefc04f4b8";
		public const string DestinationConfiguration = @"b1323ca7-34e5-4e6b-8ff1-e8d3b1a5fd0a";
		public const string SourceProvider = @"dc902551-2c9c-4f41-a917-41f4a3ef7409";
		public const string ScheduleRule = @"000f25ef-d714-4671-8075-d2a71cac396b";
		public const string OverwriteFields = @"0cae01d8-0dc3-4852-9359-fb954215c36f";
		public const string DestinationProvider = @"d6f4384a-0d2c-4eee-aab8-033cc77155ee";
		public const string Name = @"d534f433-dd92-4a53-b12d-bf85472e6d7a";
	}



	public partial class SourceProviderFields : BaseFields
	{
		public const string Identifier = @"Identifier";
		public const string SourceConfigurationUrl = @"Source Configuration Url";
		public const string LibLocation = @"LibLocation";
		public const string Name = @"Name";
	}

	public partial class SourceProviderFieldGuids 
	{
		public const string Identifier = @"d0ecc6c9-472c-4296-83e1-0906f0c0fbb9";
		public const string SourceConfigurationUrl = @"b1b34def-3e77-48c3-97d4-eae7b5ee2213";
		public const string LibLocation = @"8eedeec9-eebf-403b-972d-31a0d7362f08";
		public const string Name = @"9073997b-319e-482f-92fe-67e0b5860c1b";
	}



	public partial class DestinationProviderFields : BaseFields
	{
		public const string Identifier = @"Identifier";
		public const string Name = @"Name";
	}

	public partial class DestinationProviderFieldGuids 
	{
		public const string Identifier = @"9fa104ac-13ea-4868-b716-17d6d786c77a";
		public const string Name = @"3ed18f54-c75a-4879-92a8-5ae23142bbeb";
	}



	#endregion

	#region "Choice Constants"

	#endregion								

	#region "Layouts"

	public partial class IntegrationPointLayoutGuids
	{
		public const string IntegrationPointDetails = @"f4a9ed1f-d874-4b07-b127-043e8ad0d506";
	}

	public partial class IntegrationPointLayouts
	{
		public const string IntegrationPointDetails = @"Integration Point Details";
	}

	public partial class SourceProviderLayoutGuids
	{
		public const string SourceProviderLayout = @"6d2ecb5d-ec2d-4b4b-b631-47fada8af8d4";
	}

	public partial class SourceProviderLayouts
	{
		public const string SourceProviderLayout = @"Source Provider Layout";
	}

	public partial class DestinationProviderLayoutGuids
	{
		public const string DestinationProviderLayout = @"806a3f21-3171-4093-afe6-b7a53cd2c4b5";
	}

	public partial class DestinationProviderLayouts
	{
		public const string DestinationProviderLayout = @"DestinationProvider Layout";
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

	public partial class DestinationProviderTabGuids
	{
		public const string DestinationProvider = @"8ee118d4-f0b4-4db5-ab1b-ad1d86ac8564";
	}

	public partial class DestinationProviderTabs
	{
		public const string DestinationProvider = @"DestinationProvider";
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

	public partial class DestinationProviderViewGuids
	{
		public const string AllDestinationProviders = @"602c03fd-3694-4547-ab39-598a95a957d2";
	}

	public partial class DestinationProviderViews
	{
		public const string AllDestinationProviders = @"All DestinationProviders";
	}

	#endregion									

}
