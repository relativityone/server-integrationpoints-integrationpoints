# RIP Memory usage monitoring in order to catch leaks

## Status

Proposed

## Context

When customer is pushing bigger jobs, RIP process in Agent is occupingy all avaible memeory on Host box. This is problematic, as all other Agents hosted on that machine are unable to work properly.
As RIP is hosted inside Agent framework, we don't have effective way to correlate RIP memory usage with currently running job and with actions that happens inside job.

## Report Memory Usage as Metric
Proposed solution:
1. Send APM (NewRelic) metrics with memery usage and RIP job ID and job type.

Proposed class:
```{csharp}
    public class MemoryUsageReporter: IMemoryUsageReporter
    {
        private const int _TIMER_INTERVAL_MS = 30 * 1000;
        private Timer _timerThread;
        private IAPM _apmClient;

        public MemoryUsageReporter(IAPM apmClient)
        {
            _timerThread = new Timer(state => Execute(), null, Timeout.Infinite, Timeout.Infinite);
            apmClient = _apmClient;
        }

        public IDisposable ActivateTimer()
        {
            _timerThread.Change(0, _TIMER_INTERVAL_MS);

            return Disposable.Create(() =>
            {
                _timerThread.Change(Timeout.Infinite, Timeout.Infinite);
            });
        }

        private void Execute()
        {
            var currentProcess = Process.GetCurrentProcess();
            long memoryUsage = currentProcess.PrivateMemorySize64;
            //Create Dictionery with memory usage and JobDetails
            // public void Log(string name, Dictionary<string, object> customData)
            // {
            //     _apm.CountOperation(name, customData: customData).Write();
            // }
            _apmClient.CountOperation("Relativity.IntegrationPoints.Performance.MemUsage", customData: Dictionary<string, object>);
        }
    }

    public interface IMemoryUsageReporter
    {
        IDisposable ActivateTimer();
    }
```
```{csharp}
Agent.cs
.
.
protected override TaskResult ProcessJob(Job job)
{
    SetWebApiTimeout();

    using (IWindsorContainer ripContainerForSync = CreateAgentLevelContainer())
    using (ripContainerForSync.Resolve<IJobContextProvider>().StartJobContext(job))
    using (ripContainerForSync.Resolve<IMemoryUsageReporter>().ActivateTimer())
```

Currently old RIP is sending following metric every 30sec: `IntegrationPoints.Performance.Progress`. This APM metrics contains infomrmatio about throughput of metadata, file and total transfer from last 30 sec.
We should be safe to add another metric with details of systme usage.
Proposed Details:
 - Memory usage
 - Job ID
 - Job Type
 - Name: `IntegrationPoints.Performance.System`

## Consequences

We are only sending one extra metric per 30 sec per job.
With this kind of information, we should be able to easily configure alert for Sync high memory usage jobs, and with this alert we could grab memory dumps.
