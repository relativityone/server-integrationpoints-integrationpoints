using Autofac;
using Relativity.Sync.Configuration;
using Relativity.Sync.ExecutionConstrains;
using Relativity.Sync.Executors.Validation;

namespace Relativity.Sync.Executors
{
	internal sealed class ExecutorsInstaller : IInstaller
	{
		public void Install(ContainerBuilder builder)
		{
			builder.RegisterType<SourceCaseTagService>().As<ISourceCaseTagService>();
			builder.RegisterType<RelativitySourceCaseTagRepository>().As<IRelativitySourceCaseTagRepository>();
			builder.RegisterType<SourceJobTagService>().As<ISourceJobTagService>();
			builder.RegisterType<JobHistoryNameQuery>().As<IJobHistoryNameQuery>();
			builder.RegisterType<RelativitySourceJobTagRepository>().As<IRelativitySourceJobTagRepository>();
			builder.RegisterType<DestinationWorkspaceTagRepository>().As<IDestinationWorkspaceTagRepository>();
			builder.RegisterType<DestinationWorkspaceTagLinker>().As<IDestinationWorkspaceTagsLinker>();
			builder.RegisterType<FederatedInstance>().As<IFederatedInstance>();
			builder.RegisterType<WorkspaceNameQuery>().As<IWorkspaceNameQuery>();
			builder.RegisterType<TagNameFormatter>().As<ITagNameFormatter>();
			builder.RegisterType<WorkspaceNameValidator>().As<IWorkspaceNameValidator>();

			builder.RegisterType<SourceWorkspaceTagsCreationExecutionConstrains>().As<IExecutionConstrains<ISourceWorkspaceTagsCreationConfiguration>>();
			builder.RegisterType<SourceWorkspaceTagsCreationExecutor>().As<IExecutor<ISourceWorkspaceTagsCreationConfiguration>>();
			builder.RegisterType<DestinationWorkspaceTagsCreationExecutionConstrains>().As<IExecutionConstrains<IDestinationWorkspaceTagsCreationConfiguration>>();
			builder.RegisterType<DestinationWorkspaceTagsCreationExecutor>().As<IExecutor<IDestinationWorkspaceTagsCreationConfiguration>>();
		}
	}
}
