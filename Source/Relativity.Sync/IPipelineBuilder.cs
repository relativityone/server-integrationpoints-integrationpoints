using Autofac;

namespace Relativity.Sync
{
    internal interface IPipelineBuilder
    {
        void RegisterFlow(ContainerBuilder containerBuilder);
    }
}
