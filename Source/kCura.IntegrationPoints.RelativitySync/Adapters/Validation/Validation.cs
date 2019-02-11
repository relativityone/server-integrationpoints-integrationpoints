﻿using System;
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
	internal sealed class Validation : IExecutor<IValidationConfiguration>, IExecutionConstrains<IValidationConfiguration>
	{
		private readonly IAPILog _logger;
		private readonly IWindsorContainer _container;
		private readonly IExtendedJob _extendedJob;
		private readonly IValidationExecutorFactory _validationExecutorFactory;
		private readonly IRdoRepository _rdoRepository;

		public Validation(IWindsorContainer container, IValidationExecutorFactory validationExecutorFactory, IRdoRepository rdoRepository)
		{
			_container = container;
			_validationExecutorFactory = validationExecutorFactory;
			_rdoRepository = rdoRepository;
			_extendedJob = _container.Resolve<IExtendedJob>();
			_logger = _container.Resolve<IAPILog>();
		}

		public Task<bool> CanExecuteAsync(IValidationConfiguration configuration, CancellationToken token)
		{
			return Task.FromResult(true);
		}

		public async Task ExecuteAsync(IValidationConfiguration configuration, CancellationToken token)
		{
			_logger.LogDebug("Validating IntegrationPoint");

			await Task.Yield();
			ValidateIntegrationPoint();

			_logger.LogDebug("Validation successful");
		}

		private void ValidateIntegrationPoint()
		{
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

			ValidationContext context = new ValidationContext
			{
				DestinationProvider = destinationProvider,
				IntegrationPointType = integrationPointType,
				Model = IntegrationPointModel.FromIntegrationPoint(integrationPoint),
				ObjectTypeGuid = ObjectTypeGuids.IntegrationPoint,
				SourceProvider = sourceProvider,
				UserId = -1 // User permissions check is a separate step in Sync flow.
							// Putting -1 as UserId here, allows ValidationExecutor to pass
							// successfully (because it's supposed to validate only IntegrationPoint model,
							// not user permissions).
			};

			IValidationExecutor validationExecutor = _validationExecutorFactory.CreateProviderValidationExecutor();
			validationExecutor.ValidateOnRun(context);
		}
	}
}