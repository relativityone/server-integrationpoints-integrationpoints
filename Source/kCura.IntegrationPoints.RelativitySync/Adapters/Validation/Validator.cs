using System;
using System.Threading;
using System.Threading.Tasks;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
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
		private readonly int _integrationPointId;
		private readonly IValidationExecutorFactory _validationExecutorFactory;

		public Validator(IWindsorContainer container, int integrationPointId, IValidationExecutorFactory validationExecutorFactory)
		{
			_container = container;
			_integrationPointId = integrationPointId;
			_validationExecutorFactory = validationExecutorFactory;
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
			IntegrationPoint integrationPoint = GetIntegrationPoint();

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

			SourceProvider sourceProvider = ReadObject<SourceProvider>(integrationPoint.SourceProvider.Value);
			DestinationProvider destinationProvider = ReadObject<DestinationProvider>(integrationPoint.DestinationProvider.Value);
			IntegrationPointType integrationPointType = ReadObject<IntegrationPointType>(integrationPoint.Type.Value);

			ValidationContext context = new ValidationContext
			{
				DestinationProvider = destinationProvider,
				IntegrationPointType = integrationPointType,
				Model = IntegrationPointModel.FromIntegrationPoint(integrationPoint),
				ObjectTypeGuid = ObjectTypeGuids.IntegrationPoint,
				SourceProvider = sourceProvider,
				UserId = -1
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

		private IntegrationPoint GetIntegrationPoint()
		{
			try
			{
				_logger.LogDebug("Retrieving Integration Point with id: {integrationPointId}", _integrationPointId);
				IIntegrationPointService integrationPointService = _container.Resolve<IIntegrationPointService>();
				IntegrationPoint integrationPoint = integrationPointService.GetRdo(_integrationPointId);
				_logger.LogInformation("Integration Point with id: {integrationPointId} retrieved successfully.", _integrationPointId);
				return integrationPoint;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to retrieve Integration Point with id: {integrationPointId}", _integrationPointId);
				throw;
			}
		}
	}
}