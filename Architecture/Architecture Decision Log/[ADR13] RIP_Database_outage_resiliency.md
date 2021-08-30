# RIP Database outage resiliency

## Status

Proposed

## Context

In the event of a database failover, there will be an outage of the SQL PaaS system that will last, on average, for 1 minute. What we would like to understand from teams is the potential impact of this outage on your products/applications.

In our current product(RIP) we are using database connection without any wrapper on it. Our product does not have any retries or any resilience implemented against database outage.
RIP job queue functionality is based on ScheduleAgentQueue and JobHistory table.
There are many actions on this tables happening, here are links for SQL scripts:
https://git.kcura.com/projects/IN/repos/integrationpoints/browse/Source/kCura.IntegrationPoints.Data/Resources
https://git.kcura.com/projects/IN/repos/integrationpoints/browse/Source/kCura.ScheduleQueue.Core/Data/Resources

### RIP Job Phases:

1. Starting job:  
During this phase, if next job is not picked, Agent will just not find any jobs, and shut down.
If job was picked, but db shutdown just before creating job history for given job, this Job is broken, and cannot be finished properly.
2. Sending data:  
 During this phase we are mainly updating number of records sent, so if db will be not available, this will be updated next time it's on. But I have no idea if code will not break when trying to update this numbers.(probably yes) If job will break, it will be needed to re rerun job again.
3. Finishing job:  
If db is down, we will not be able to put proper state of job in db, so it will be eg. In Progress and this will be visible on UI despite actual job finishing.
If job is on schedule, RIP will not be able to put another job with new calculated time to run in queue effectively destroying this functionality.

### Summary:

All in all, for RIP to properly work, we would need to implement some sort of retries mechanism(Polly or smth). With 60 sec outages there is really high chance that during data transfer between workspace job will be unable to finish, and in best case would need manual intervention in database (cleaning queue or adjusting job status), in worst case that would require creating new saved search and new Job definition.

## Proposed solution

### Proxy for DynamicProxy

We need to implement wrapper for IDBContext.
In every implementation of IServiceContextHelper we need to adjust following method:  
`public IDBContext GetDBContext() => _helper.GetDBContext(this.WorkspaceID);`  
`public IDBContext GetDBContext() => _dynamicProxy.WrapKeplerService(_helper.GetDBContext(this.WorkspaceID));`  
Use following solution from Sync:  
Relativity.Sync\KeplerFactory\DynamicProxyFactory.cs

Use KeplerServiceInterceptor.cs as it was proved to be working in Sync for over 2 years on production.  
Modify following method: HandleExceptionsAsync to catch SQL outage exceptions

Removed Report Metrics -> will be implemented later
Remove Auth Policy -> we just need one basic policy for outage -> we don't need to create new Object of Kepler

DynamicProxy
IDyanmicProxy interface
We need to register to container.
kCura.IntegrationPoints.Agent\Installer\AgentInstaller.cs
InstallContainer
Add new register of DynamicProxy
container.Register(Component.For<IDynamicProxy>().ImplementedBy<DynamicProxy>());

44 occurrences across 7 projects

We don't want to modify tests implementation of GetDbConnection.

UnitTest only DynamicProxy

### Another Idea: proxy for Helper

Write extension method.

### Problem for SQL outage: Open and Close Transactions

Check SQL scripts for transactions so we don't have inceptions of transactions and are idempotent.

Check if we don't have SQL transactions in code.
