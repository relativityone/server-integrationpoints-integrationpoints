using System;
using System.Runtime.Serialization;
using kCura.Apps.Common.Utils.Serializers;
using kCura.ScheduleQueue.Core.ScheduleRules;

namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
	[DataContract]
	public class ScheduleRule
	{
		[DataMember]
		public ScheduleInterval Interval { get; set; }

		[DataMember]
		public DateTime? StartDate { get; set; }

		[DataMember]
		public DateTime? EndDate { get; set; }

		[DataMember]
		public int? TimeZoneOffsetInMinute { get; set; }

		[DataMember]
		public string TimeZoneId { get; set; }

		[DataMember]
		public int? DayOfMonth { get; set; }

		[DataMember]
		public bool? SetLastDayOfMonth { get; set; }

		[DataMember]
		public DaysOfWeek? DaysToRun { get; set; }

		[DataMember]
		public int? Reoccur { get; set; }

		[DataMember]
		public OccuranceInMonth? OccuranceInMonth { get; set; }

		public string Serialize()
		{
			var serializer = new XMLSerializerFactory();

			return serializer.Serialize(this);
		}
	}
}
