# Creating alternative versions of Sync pipeline

## Status

Approved

## Context

Upon adding job retrying to Sync, we need the possibillity to create different, specialized versions of the execution pipeline based on job parameters.

Current implementation uses a container to resolve a ```SyncJob``` instance with ```INode<SyncExecutionContext> pipeline``` as a parameter. Only one implementation is registered.

## Decision

All pipeline versions should be wrapped seperate classes that implement the same interface:

```csharp
public interface ISyncPipeline
{
   public string Name {get;}
   public void BuildFlow(IFlowBuilder<SyncExecutionContext> flowBuilder);
}
```

All pipeline classes should be sealed (possibly a reflection unit test should guard that) and create **all nodes explicitly**.

```PipelineBuilder.RegisterFlow``` should have a ```SyncJobParameters``` parameter and register single flow based on the job parameters.

```csharp

private ISyncPipeline GetPipeline(SyncJobParameters parameters)
{
    if(IsSyncDocumentPushJob(parameters))
    {
       return new SyncDocumentPushPipeline();
    }

    if(IsSyncDocumentRetry(parameters))
    {
       return new SyncDocumentRetryPipeline();
    }

    // other possibillities
    // use pattern matching instead of ifs if possible
}

// then in RegisterFlow
{
    // ...
    ISyncPipeline pipeline = GetPipeline(parameters);
    pipeline.BuildFlow(flowBuilder.CreateFlow(pipeline.Name));
    // continue as before
}
```

Name of the pipeline should be included in logging context.

Different implementations of a node that have similar function should implement the same interface and be seperate classes.

## Consequences

*Positive:*

- there is a single point of decission
- it is easy to follow the logic of building the pipeline
- once you know which flow it is, all nodes are explicitly accessible to investigate
- ```Go to implementation``` in ```GetPipeline``` shows the nodes

*Negative:*

- the code is a little harder to maintain
  - changing nodes requires modyfying all factory methods
    - we can add reflection unit tests that check if all pipelines have proper nodes
