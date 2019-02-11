using System;
using System.Threading;
using System.Threading.Tasks;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using Relativity.API;
using Relativity.Sync;
using Relativity.Sync.Configuration;

namespace kCura.IntegrationPoints.RelativitySync.Adapters
{
	internal sealed class Validator : IExecutor<IValidationConfiguration>, IExecutionConstrains<IValidationConfiguration>
	{
		private readonly IAPILog _logger;
		private readonly IWindsorContainer _container;
		private readonly IExtendedJob _extendedJob;
		private readonly IValidationExecutorFactory _validationExecutorFactory;

		public Validator(IWindsorContainer container, IValidationExecutorFactory validationExecutorFactory)
		{
			_container = container;
			_validationExecutorFactory = validationExecutorFactory;
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
			if (!_extendedJob.IntegrationPointModel.SourceProvider.HasValue)
			{
				throw new InvalidOperationException($"SourceProvider in retrieved IntegrationPoint has no value.");
			}

			if (!_extendedJob.IntegrationPointModel.DestinationProvider.HasValue)
			{
				throw new InvalidOperationException($"DestinationProvider in retrieved IntegrationPoint has no value.");
			}

			if (!_extendedJob.IntegrationPointModel.Type.HasValue)
			{
				throw new InvalidOperationException($"Type in retrieved IntegrationPoint has no value.");
			}

			SourceProvider sourceProvider = ReadObject<SourceProvider>(_extendedJob.IntegrationPointModel.SourceProvider.Value);
			DestinationProvider destinationProvider = ReadObject<DestinationProvider>(_extendedJob.IntegrationPointModel.DestinationProvider.Value);
			IntegrationPointType integrationPointType = ReadObject<IntegrationPointType>(_extendedJob.IntegrationPointModel.Type.Value);

			ValidationContext context = new ValidationContext
			{
				DestinationProvider = destinationProvider,
				IntegrationPointType = integrationPointType,
				Model = IntegrationPointModel.FromIntegrationPoint(_extendedJob.IntegrationPointModel),
				ObjectTypeGuid = ObjectTypeGuids.IntegrationPoint,
				SourceProvider = sourceProvider,
				UserId = -1 // User permissions check is a separate step in Sync flow.
                // Putting -1 as UserId here, allows ValidationExecutor to pass
                // successfully (because it's supposed to validate only IntegrationPoint model,
                // not user permissions).
			};

			IValidationExecutor validationExecutor = _validationExecutorFactory.CreateValidationExecutor();
			validationExecutor.ValidateOnRun(context);
		}

		private T ReadObject<T>(int artifactId) where T: BaseRdo, new()
		{
			string typeName = nameof(T);
			_logger.LogDebug("Reading object of type {typeName} with artifact ID: {artifactId} using ObjectManager", typeName, artifactId);
			try
			{
				ICaseServiceContext caseServiceContext = _container.Resolve<ICaseServiceContext>();
				T readObject = caseServiceContext.RsapiService.RelativityObjectManager.Read<T>(artifactId);
				_logger.LogDebug("Successfuly retrieved object of type {typeName} with artifact ID: {artifactId} using ObjectManager", typeName, artifactId);
				return readObject;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to retrieve object of type {typeName} with artifact ID: {artifactId} using ObjectManager", typeName, artifactId);
				throw;
			}
		}
	}
}