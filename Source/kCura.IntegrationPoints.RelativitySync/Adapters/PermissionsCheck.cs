using System;
using System.Threading;
using System.Threading.Tasks;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using Relativity.API;
using Relativity.Sync;
using Relativity.Sync.Configuration;

namespace kCura.IntegrationPoints.RelativitySync.Adapters
{
	internal class PermissionsCheck : IExecutor<IPermissionsCheckConfiguration>, IExecutionConstrains<IPermissionsCheckConfiguration>
	{
		private readonly IWindsorContainer _ripContainer;
		private readonly IAPILog _logger;
		private readonly IExtendedJob _extendedJob;
		private readonly IValidationExecutorFactory _validationExecutorFactory;
		private readonly IRdoRepository _rdoRepository;

		public PermissionsCheck(IWindsorContainer ripContainer, IValidationExecutorFactory validationExecutorFactory, IRdoRepository rdoRepository)
		{
			_ripContainer = ripContainer;
			_validationExecutorFactory = validationExecutorFactory;
			_rdoRepository = rdoRepository;
			_logger = _ripContainer.Resolve<IAPILog>();
			_extendedJob = _ripContainer.Resolve<IExtendedJob>();
		}

		public Task<bool> CanExecuteAsync(IPermissionsCheckConfiguration configuration, CancellationToken token)
		{
			return Task.FromResult(true);
		}

		public async Task ExecuteAsync(IPermissionsCheckConfiguration configuration, CancellationToken token)
		{
			_logger.LogDebug("Validating permissions");

			await Task.Yield();

			IntegrationPoint integrationPoint = _rdoRepository.Get<IntegrationPoint>(_extendedJob.IntegrationPointId);

			if (!integrationPoint.SourceProvider.HasValue)
			{
				throw new InvalidOperationException($"SourceProvider in retrieved IntegrationPoint has no value.");
			}

			if (!integrationPoint.DestinationProvider.HasValue)
			{
				throw new InvalidOperationException($"DestinationProvider in retrieved IntegrationPoint has no value.");
			}

			if (!integrationPoint.Type.HasValue)
			{
				throw new InvalidOperationException($"Type in retrieved IntegrationPoint has no value.");
			}

			SourceProvider sourceProvider = _rdoRepository.Get<SourceProvider>(integrationPoint.SourceProvider.Value);
			DestinationProvider destinationProvider = _rdoRepository.Get<DestinationProvider>(integrationPoint.DestinationProvider.Value);
			IntegrationPointType integrationPointType = _rdoRepository.Get<IntegrationPointType>(integrationPoint.Type.Value);

			IntegrationPointModelBase model = IntegrationPointModel.FromIntegrationPoint(integrationPoint);

			ValidationContext context = new ValidationContext()
			{
				SourceProvider = sourceProvider,
				DestinationProvider = destinationProvider,
				IntegrationPointType = integrationPointType,
				Model = model,
				ObjectTypeGuid = ObjectTypeGuids.IntegrationPoint,
				UserId = configuration.ExecutingUserId
			};

			IValidationExecutor validationExecutor = _validationExecutorFactory.CreatePermissionValidationExecutor();
			validationExecutor.ValidateOnRun(context);

			_logger.LogDebug("Validation successful");
		}
	}
}
