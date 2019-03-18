using Autofac;
using Relativity.Sync.Configuration;
using Relativity.Sync.ExecutionConstrains;

namespace Relativity.Sync.Executors
{
	internal sealed class ExecutorsInstaller : IInstaller
	{
		public void Install(ContainerBuilder builder)
		{
			builder.RegisterType<DestinationWorkspaceTagRepository>().As<IDestinationWorkspaceTagRepository>();
			builder.RegisterType<DestinationWorkspaceTagsLinker>().As<IDestinationWorkspaceTagsLinker>();
			builder.RegisterType<FederatedInstance>().As<IFederatedInstance>();
			builder.RegisterType<WorkspaceNameQuery>().As<IWorkspaceNameQuery>();

			builder.RegisterType<SourceWorkspaceTagsCreationExecutor>().As<IExecutor<ISourceWorkspaceTagsCreationConfiguration>>();
			builder.RegisterType<SourceWorkspaceTagsCreationExecutionConstrains>().As<IExecutionConstrains<ISourceWorkspaceTagsCreationConfiguration>>();
		}
	}
}
