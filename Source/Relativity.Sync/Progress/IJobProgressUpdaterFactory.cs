namespace Relativity.Sync.Progress
{
    internal interface IJobProgressUpdaterFactory
    {
        IJobProgressUpdater CreateJobProgressUpdater();
    }
}
