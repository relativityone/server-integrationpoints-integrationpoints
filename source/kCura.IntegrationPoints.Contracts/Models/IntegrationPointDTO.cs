using System;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.Contracts.Models
{
	public class IntegrationPointDTO
	{
		public int ArtifactId { get; set; }
		public string DestinationConfiguration { get; set; }
		public int? DestinationProvider { get; set; }
		public string EmailNotificationRecipients { get; set; }
		public bool? EnableScheduler { get; set; }
		public string FieldMappings { get; set; }
		public bool? HasErrors { get; set; }
		public int[] JobHistory { get; set; }
		public DateTime? LastRuntimeUTC { get; set; }
		public bool? LogErrors { get; set; }
		public string Name { get; set; }
		public DateTime? NextScheduledRuntimeUTC { get; set; }
		public List<Choices.OverwriteFields.Values> OverwriteFields { get; set; }
		public string ScheduleRule { get; set; }
		public string SourceConfiguration { get; set; }
		public int? SourceProvider { get; set; }

		public class FieldGuids
		{
			public const string DestinationConfiguration = @"b1323ca7-34e5-4e6b-8ff1-e8d3b1a5fd0a";
			public const string DestinationProvider = @"d6f4384a-0d2c-4eee-aab8-033cc77155ee";
			public const string EmailNotificationRecipients = @"1bac59db-f7bf-48e0-91d4-18cf09ff0e39";
			public const string EnableScheduler = @"bcdafc41-311e-4b66-8084-4a8e0f56ca00";
			public const string FieldMappings = @"1b065787-a6e4-4d70-a7ed-f49d770f0bc7";
			public const string HasErrors = @"A9853E55-0BA0-43D8-A766-747A61471981";
			public const string JobHistory = @"14b230cf-a505-4dd3-b05c-c54d05e62966";
			public const string LastRuntimeUTC = @"90d58af1-f79f-40ae-85fc-7e42f84dbcc1";
			public const string LogErrors = @"0319869e-37aa-499c-a95b-6d8d0e96a711";
			public const string Name = @"d534f433-dd92-4a53-b12d-bf85472e6d7a";
			public const string NextScheduledRuntimeUTC = @"5b1c9986-f166-40e4-a0dd-a56f185ff30b";
			public const string OverwriteFields = @"0cae01d8-0dc3-4852-9359-fb954215c36f";
			public const string ScheduleRule = @"000f25ef-d714-4671-8075-d2a71cac396b";
			public const string SourceConfiguration = @"b5000e91-82bd-475a-86e9-32fefc04f4b8";
			public const string SourceProvider = @"dc902551-2c9c-4f41-a917-41f4a3ef7409";
		}

		public static class Choices
		{
			public static class OverwriteFields
			{
				public enum Values
				{
					AppendOnly,
					AppendOverlay,
					OverlayOnly
				}

				public static readonly Dictionary<Guid, Values> GuidValues = new Dictionary<Guid, Values>()
				{
					{new Guid("998C2B04-D42E-435B-9FBA-11FEC836AAD8"), Values.AppendOnly},
					{new Guid("5450EBC3-AC57-4E6A-9D28-D607BBDCF6FD"), Values.AppendOverlay},
					{new Guid("70A1052D-93A3-4B72-9235-AC65F0D5A515"), Values.OverlayOnly},
				};
			}
		}
	}
}
