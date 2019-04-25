using Autofac;
using Relativity.Sync.Configuration;
using Relativity.Sync.ExecutionConstrains;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.Storage;

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
			builder.RegisterType<TagSavedSearch>().As<ITagSavedSearch>();
			builder.RegisterType<TagSavedSearchFolder>().As<ITagSavedSearchFolder>();
			builder.RegisterType<BatchProgressHandlerFactory>().As<IBatchProgressHandlerFactory>();

			builder.RegisterType<SourceWorkspaceTagsCreationExecutionConstrains>().As<IExecutionConstrains<ISourceWorkspaceTagsCreationConfiguration>>();
			builder.RegisterType<SourceWorkspaceTagsCreationExecutor>().As<IExecutor<ISourceWorkspaceTagsCreationConfiguration>>();
			builder.RegisterType<DestinationWorkspaceTagsCreationExecutionConstrains>().As<IExecutionConstrains<IDestinationWorkspaceTagsCreationConfiguration>>();
			builder.RegisterType<DestinationWorkspaceTagsCreationExecutor>().As<IExecutor<IDestinationWorkspaceTagsCreationConfiguration>>();
			builder.RegisterType<ValidationExecutionConstrains>().As<IExecutionConstrains<IValidationConfiguration>>();
			builder.RegisterType<ValidationExecutor>().As<IExecutor<IValidationConfiguration>>();
			builder.RegisterType<DestinationWorkspaceSavedSearchCreationExecutionConstrains>().As<IExecutionConstrains<IDestinationWorkspaceSavedSearchCreationConfiguration>>();
			builder.RegisterType<DestinationWorkspaceSavedSearchCreationExecutor>().As<IExecutor<IDestinationWorkspaceSavedSearchCreationConfiguration>>();
			builder.RegisterType<DataSourceSnapshotExecutor>().As<IExecutor<IDataSourceSnapshotConfiguration>>();
			builder.RegisterType<DataSourceSnapshotExecutionConstrains>().As<IExecutionConstrains<IDataSourceSnapshotConfiguration>>();

			builder.RegisterType<BatchRepository>().As<IBatchRepository>();
			builder.RegisterType<ProgressRepository>().As<IProgressRepository>();
			builder.RegisterType<SemaphoreSlimWrapper>().As<ISemaphoreSlim>();
		}
	}
}