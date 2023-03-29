using System;
using System.Security.Authentication;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Common.Handlers;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI;
using kCura.IntegrationPoints.Synchronizers.RDO.Properties;
using kCura.Relativity.ImportAPI;
using kCura.WinEDDS.Exceptions;
using Relativity.IntegrationPoints.FieldsMapping.ImportApi;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
    public sealed class ImportApiFactory : IImportApiFactory
    {
        private readonly IInstanceSettingsManager _instanceSettingsManager;
        private readonly IImportApiBuilder _importApiBuilder;
        private readonly IRetryHandler _retryHandler;
        private readonly ILogger<ImportApiFactory> _logger;

        public ImportApiFactory(
            IInstanceSettingsManager instanceSettingsManager,
            IImportApiBuilder importApiBuilder,
            IRetryHandlerFactory retryHandlerFactory,
            ILogger<ImportApiFactory> logger)
        {
            _instanceSettingsManager = instanceSettingsManager;
            _importApiBuilder = importApiBuilder;
            _retryHandler = retryHandlerFactory.Create(7, 2);
            _logger = logger;
        }

        public IImportAPI GetImportAPI(string webServiceUrl)
        {
            try
            {
                return _retryHandler.Execute<IImportAPI, Exception>(
                    () => CreateImportAPI(webServiceUrl),
                    exception =>
                    {
                        _logger.LogWarning(
                            exception,
                            "Failed to create Import API for url: {webServiceUrl}. Operation will be retried.",
                            webServiceUrl);
                    });
            }
            catch (InvalidLoginException ex)
            {
                LogLoginFailed(ex, webServiceUrl);
                var authException = new AuthenticationException(ErrorMessages.Login_Failed, ex);
                throw new IntegrationPointsException(ErrorMessages.Login_Failed, authException)
                {
                    ShouldAddToErrorsTab = true,
                    ExceptionSource = IntegrationPointsExceptionSource.IAPI
                };
            }
            catch (Exception ex)
            {
                LogCreatingImportApiError(ex, webServiceUrl);
                throw;
            }
        }

        public IImportApiFacade GetImportApiFacade(string webServiceUrl)
        {
            return new ImportApiFacade(this, webServiceUrl, _logger.ForContext<ImportApiFacade>());
        }

        private IImportAPI CreateImportAPI(string webServiceUrl)
        {
            IImportAPI importApi = _importApiBuilder.CreateImportAPI(webServiceUrl, _instanceSettingsManager.GetIApiBatchSize());
            LogImportApiCreated();
            return importApi;
        }

        #region Logging

        private void LogCreatingImportApiError(Exception ex, string url)
        {
            _logger.LogError(ex, "Failed to create Import API ({URL}).", url);
        }

        private void LogLoginFailed(Exception ex, string url)
        {
            _logger.LogError(ex, "Failed to create Import API ({URL}): Login failed.", url);
        }

        private void LogImportApiCreated()
        {
            _logger.LogInformation("Successfully created Import API.");
        }

        #endregion
    }
}
