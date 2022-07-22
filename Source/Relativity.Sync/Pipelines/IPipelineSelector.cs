namespace Relativity.Sync.Pipelines
{
    internal interface IPipelineSelector
    {
        ISyncPipeline GetPipeline();
    }
}
