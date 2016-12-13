using kCura.IntegrationPoints.Core.Models;

namespace kCura.IntegrationPoints.Services.Extensions
{
	public static class SheduleModelExtensions
	{
		public static Scheduler ToScheduler(this ScheduleModel model)
		{
			return new Scheduler
			{
				EnableScheduler = model.EnableScheduler,
				TimeZoneOffsetInMinute = model.TimeZoneOffsetInMinute,
				Reoccur = model.Reoccur,
				StartDate = model.StartDate,
				ScheduledTime = model.ScheduledTime,
				SendOn = model.SendOn,
				EndDate = model.EndDate,
				SelectedFrequency = model.SelectedFrequency,
				TimeZoneId = model.TimeZoneId
			};
		}
	}
}