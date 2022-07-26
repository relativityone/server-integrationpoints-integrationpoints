namespace Relativity.Sync
{
    internal interface IJobProgressUpdaterFactory
    {
        IJobProgressUpdater CreateJobProgressUpdater();
    }
}