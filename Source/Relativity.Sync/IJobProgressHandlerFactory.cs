namespace Relativity.Sync
{
	internal interface IJobProgressHandlerFactory
	{
		IJobProgressHandler CreateJobProgressHandler(IJobProgressUpdater jobProgressUpdater);
	}
}