using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Models;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public class SendEmailWorker : ITask
	{
		private readonly ISerializer _serializer;
		public SendEmailWorker(ISerializer serializer)
		{
			_serializer = serializer;
		}

		public void Execute(Job job)
		{
			var details = _serializer.Deserialize<List<EmailMessage>>(job.JobDetails);

			throw new NotImplementedException();
		}

	}
}
