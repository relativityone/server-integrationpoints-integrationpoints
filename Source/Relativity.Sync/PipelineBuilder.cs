using System;
using Autofac;
using Banzai;
using Banzai.Autofac;
using Banzai.Factories;
using Relativity.Sync.Pipelines;

namespace Relativity.Sync
{
    internal sealed class PipelineBuilder : IPipelineBuilder
    {
        public void RegisterFlow(ContainerBuilder containerBuilder)
        {
            if (containerBuilder == null)
            {
                throw new ArgumentNullException(nameof(containerBuilder));
            }

            containerBuilder.RegisterBanzaiNodes(GetType().Assembly, true);

            RegisterPipelines(containerBuilder);

            containerBuilder.Register(componentContext =>
                {
                    ISyncPipeline pipeline = componentContext.Resolve<IPipelineSelector>().GetPipeline();
                    return componentContext.Resolve<INodeFactory<SyncExecutionContext>>().BuildFlow(pipeline.GetType().Name);
                })
                .As<INode<SyncExecutionContext>>();
        }

        private void RegisterPipelines(ContainerBuilder containerBuilder)
        {
            var pipelines = new ISyncPipeline[] { new SyncDocumentRunPipeline(), new SyncDocumentRetryPipeline(), new SyncImageRunPipeline(), new SyncImageRetryPipeline(), new SyncNonDocumentRunPipeline() };

            foreach (ISyncPipeline syncPipeline in pipelines)
            {
                FlowBuilder<SyncExecutionContext> flowBuilder = new FlowBuilder<SyncExecutionContext>(new AutofacFlowRegistrar(containerBuilder));

                syncPipeline.BuildFlow(flowBuilder.CreateFlow(syncPipeline.GetType().Name));

                flowBuilder.Register();
            }
        }
    }
}