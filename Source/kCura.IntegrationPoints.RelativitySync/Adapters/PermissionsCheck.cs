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
		private readonly IAPILog _logger;
		private readonly IExtendedJob _extendedJob;
		private readonly IValidationExecutorFactory _validationExecutorFactory;
		private readonly IRdoRepository _rdoRepository;

		public PermissionsCheck(IWindsorContainer ripContainer, IValidationExecutorFactory validationExecutorFactory, IRdoRepository rdoRepository)
		{
			_validationExecutorFactory = validationExecutorFactory;
			_rdoRepository = rdoRepository;
			_logger = ripContainer.Resolve<IAPILog>();
			_extendedJob = ripContainer.Resolve<IExtendedJob>();
		}

		public Task<bool> CanExecuteAsync(IPermissionsCheckConfiguration configuration, CancellationToken token)
		{
			return Task.FromResult(true);
		}

		public async Task<ExecutionResult> ExecuteAsync(IPermissionsCheckConfiguration configuration, CancellationToken token)
		{
			_logger.LogDebug("Validating permissions");

			await Task.Yield();
			ValidatePermissions(configuration);

			_logger.LogDebug("Validation successful");
			
			return ExecutionResult.Success();
		}

		private void ValidatePermissions(IPermissionsCheckConfiguration configuration)
		{
			if (!_extendedJob.IntegrationPointModel.SourceProvider.HasValue)
			{
				throw new InvalidOperationException("SourceProvider in retrieved IntegrationPoint has no value.");
			}

			if (!_extendedJob.IntegrationPointModel.DestinationProvider.HasValue)
			{
				throw new InvalidOperationException("DestinationProvider in retrieved IntegrationPoint has no value.");
			}

			if (!_extendedJob.IntegrationPointModel.Type.HasValue)
			{
				throw new InvalidOperationException("Type in retrieved IntegrationPoint has no value.");
			}

			SourceProvider sourceProvider = _rdoRepository.Get<SourceProvider>(_extendedJob.IntegrationPointModel.SourceProvider.Value);
			DestinationProvider destinationProvider = _rdoRepository.Get<DestinationProvider>(_extendedJob.IntegrationPointModel.DestinationProvider.Value);
			IntegrationPointType integrationPointType = _rdoRepository.Get<IntegrationPointType>(_extendedJob.IntegrationPointModel.Type.Value);

			IntegrationPointModelBase model = IntegrationPointModel.FromIntegrationPoint(_extendedJob.IntegrationPointModel);

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
		}
	}
}
