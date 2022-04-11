# Using RelativityServiceBus in Agent

## Status

Delayed - not enough value in changes until the problem is real

## Context

Due to problems with multiple agents locking the same job, we need to improve the locking mechanism. After consulatations with tech leads, we decided to use [Relativity Service Bus](https://einstein.kcura.com/display/DV/Relativity+Service+Bus).

## Decision

I created a sample repository showing how use basic functions of the queue: [https://git.kcura.com/projects/IN/repos/relativitybusspike/browse](https://git.kcura.com/projects/IN/repos/relativitybusspike/browse). It is runnable in [Relativity Local Debugger](https://platform.relativity.com/RelativityOne/Content/Downloads/Local_debugger.htm).

Things to consider:

- There need to be an event handler that ensures `Topic` and `Subscription` creation
- Each message type is processed seperately, so we can use the same subscription and topic to handle all types of messages
- Agent should try to pool single message in `Execute` method, just like before

Taking into consideration our plans to create Queue Monitor (a page that displays running jobs accross all workspaces), I propose to only replace the locking mechanism, since creating the monitor based solely on the queue would be VERY compilcated.

Changes to make:

### Job service

Message type:
```
public class NewJobInQueue
{
    public int JobId { get; set; } // JobId to corelate with queue table
    public DateTime NextRunTimeUtc { get; set; } // to filter out jobs scheduled for future
} 
```


#### Job creation

Everything stays the same, but after writing to the table, we need to also publish a message.

The hard part would be pushing new scheduled job for the same Integration Point, which would require the service to "consume" (`await message.CompleteAsync()`) the previous message. 

#### Job pooling and locking

In `JobService.GetNextQueueJob` we should do something like this:

```csharp
private async Task AsyncExecute()
{
    IManage manager = BusPool.Bus.GetManager();
    ISubscriptionDetails subDetails = manager.GetSubscriptionAsync(BusConstants.Topic, BusConstants.Subscription).GetAwaiter().GetResult();
    ISubscriber<NewJobInQueue> sub = BusPool.Bus.GetSubscriber<NewJobInQueue>(subDetails);

    int nubmerOfMessages = 1000; // this should be an instance setting at least
    IEnumerable<IMessage<NewJobInQueue>> messages = await sub.ReceiveBatchAsync(nubmerOfMessages, TimeSpan.FromSeconds(10)); // timeout is needed to drop out if there is no message in queue, otherwise single execute would be blocked here on waiting for a job

    // no messages in the queue
    if (messages != null && messages.Any())
    {
        IMessage<NewJobInQueue> message = messages
            .FirstOrDefault(x => x.GetBody().NextRunTimeUtc > DateTime.UtcNow)

        if (message != null)
        {
            Job job = await GetJobById(message.GetBody().JobId);
            if (job != null)
            {
                // this takes message out of the queue
                // if successful we can, lock the job in the table 
                // throws exception if message was already taken by other agent, so we cannot lock twice
                await message.CompleteAsync();
                LockJob(job.JobId);

                // this part will depend on how we want to handle scheduling, see below
                if(JobIsScheduled(job))
                {
                    Job nextJob = CreateRowInQueueTableForNextSchedule();
                    await PublishMessage(new NewJobInQueue{ JobId = nextJob.JobId, NextUtcRuntime = nextJob.NextUtcRuntime });
                }

                DoTheJob();
            }
            // also finish
        }
    }
    
    // finish executing
}
```

## Scheduled jobs

Here we have two options:
- like above, we always scan the queue until we find a message that should be executed due to its `NextUtcRuntime`
  - cons:
    - if there is a lot of scheduled jobs, each agent will cycle through them before it finds more current job
  - pros:
    - easy to develop
    - has a chance to be reliable
- seperate message type for scheduled jobs
  - cons:
    - small increase in complexity
  - pros:
    - it will be very easy to steer priorities between scheduled/run jobs, just try to pool given type first
    - we could even create seperate pool of agents for scheduled jobs
- seperate message for schedule, but not representing a job (**my preferred solution**)
  - agent would first check for messages of type `ScheduledJob`, if the `NextUtcRuntime` property would indicate that it's time to execute, it would consume given `ScheduledJob` message, push new one with new `NextUtcRuntime` and push messages for normal jobs (`NewJobInQueue`)
  - cons: 
    - may seem complex, but it's two message types, each with own clear purpose
  - pros:
    - reliable scheduling
    - we could use `ReceiveAsync` instead of `ReceiveBatchAsync` for job messages (but we would still need `ReceiveBatchAsync` for the schedule messages)

## Future

In future, it would be nice to move the `ScheduleAgentQueue` table to CosmosDB, to be able to freely work on its shape. For now, it needs to stay as is.

There are global RDOs, but we create quite a lot of jobs, and it would waste tons of ArtifactID values.

## Consequences

As a consequence of this work, we would achieve reliable way for agents to lock jobs. Given how small the change is, it is well worth to invest in this solution. 