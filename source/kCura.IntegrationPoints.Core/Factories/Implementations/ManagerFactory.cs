using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;

namespace kCura.IntegrationPoints.Core.Factories.Implementations
{
	public class ManagerFactory : IManagerFactory
	{
		public IIntegrationPointManager CreateIntegrationPointManager(IContextContainer contextContainer)
		{
			return new IntegrationPointManager(CreateRepositoryFactory(contextContainer));
		}

		public IQueueManager CreateQueueManager(IContextContainer contextContainer)
		{
			return new QueueManager(CreateRepositoryFactory(contextContainer));
		}
		
		public ISourceProviderManager CreateSourceProviderManager(IContextContainer contextContainer)
		{
			return new SourceProviderManager(CreateRepositoryFactory(contextContainer));
		}

		public IFieldManager CreateFieldManager(IContextContainer contextContainer)
		{
			return new FieldManager(CreateRepositoryFactory(contextContainer));
		}

		public IJobHistoryManager CreateJobHistoryManager(IContextContainer contextContainer)
		{
			return new JobHistoryManager(CreateRepositoryFactory(contextContainer));
		}

		#region Private Helpers

		private IRepositoryFactory CreateRepositoryFactory(IContextContainer contextContainer)
		{
			return new RepositoryFactory(contextContainer.Helper);
		}

		#endregion

	}
}
