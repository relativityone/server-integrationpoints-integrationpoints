using kCura.Apps.Common.Utils.Serializers;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using Newtonsoft.Json.Linq;

namespace kCura.IntegrationPoints.Agent.Utils
{
	public static class JobParametersDeserializationUtils
	{
		public static T Deserialize<T>(Job job, ISerializer serializer)
		{
			TaskParameters taskParameters = serializer.Deserialize<TaskParameters>(job.JobDetails);

			return taskParameters.BatchParameters is JObject jObject 
				? jObject.ToObject<T>() 
				: (T)taskParameters.BatchParameters;
		}
	}
}
