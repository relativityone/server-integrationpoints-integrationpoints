using Castle.Windsor;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using Relativity.API;

namespace kCura.IntegrationPoints.RelativitySync.Adapters
{
	public class ValidationExecutorFactory : IValidationExecutorFactory
	{
		private readonly IWindsorContainer _container;

		public ValidationExecutorFactory(IWindsorContainer container)
		{
			_container = container;
		}

		public IValidationExecutor CreateProviderValidationExecutor()
		{
			return new ValidationExecutor(_container.Resolve<IIntegrationPointProviderValidator>(), new EmptyPermissionValidator(), _container.Resolve<IHelper>());
		}

		public IValidationExecutor CreatePermissionValidationExecutor()
		{
			return new ValidationExecutor(new EmptyProviderValidator(), _container.Resolve<IIntegrationPointPermissionValidator>(), _container.Resolve<IHelper>());

		}
	}
}