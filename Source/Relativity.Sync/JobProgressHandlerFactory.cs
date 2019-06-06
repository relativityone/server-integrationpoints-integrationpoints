namespace Relativity.Sync
{
	internal sealed class JobProgressHandlerFactory : IJobProgressHandlerFactory
	{
		private readonly IDateTime _dateTime;

		public JobProgressHandlerFactory(IDateTime dateTime)
		{
			_dateTime = dateTime;
		}

		public IJobProgressHandler CreateJobProgressHandler(IJobProgressUpdater jobProgressUpdater)
		{
			return new JobProgressHandler(jobProgressUpdater, _dateTime);
		}
	}
}