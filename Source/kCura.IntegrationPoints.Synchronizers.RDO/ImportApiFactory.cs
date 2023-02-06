using System;
using System.Security.Authentication;
using kCura.IntegrationPoints.Domain.Authentication;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI;
using kCura.IntegrationPoints.Synchronizers.RDO.Properties;
using kCura.Relativity.ImportAPI;
using kCura.Relativity.ImportAPI.Enumeration;
using kCura.WinEDDS.Exceptions;
using Polly;
using Polly.Retry;
using Relativity.API;
using Relativity.DataExchange;
using Relativity.IntegrationPoints.FieldsMapping.ImportApi;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
    public class ImportApiFactory : IImportApiFactory
    {
        private readonly IInstanceSettingsManager _instanceSettingsManager;
        private readonly IAPILog _logger;
        private readonly IAuthTokenGenerator _authTokenGenerator;

        public ImportApiFactory(
            IAuthTokenGenerator authTokenGenerator,
            IInstanceSettingsManager instanceSettingsManager,
            IAPILog logger)
        {
            _authTokenGenerator = authTokenGenerator;
            _instanceSettingsManager = instanceSettingsManager;
            _logger = logger.ForContext<ImportApiFactory>();
        }

        public virtual IImportAPI GetImportAPI(ImportSettings settings)
        {
            LogImportSettings(settings);

            const int maxRetryCount = 7;

            RetryPolicy policy = Policy
                .Handle<Exception>()
                .WaitAndRetry(retryCount: maxRetryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(ex, "Failed to create Import API. Retry {retry} of {maxRetryCount}", retryCount, maxRetryCount);
                });

            try
            {
                return policy.Execute(() => CreateImportAPI(settings));
            }
            catch (InvalidLoginException ex)
            {
                LogLoginFailed(ex, settings.WebServiceURL);
                var authException = new AuthenticationException(ErrorMessages.Login_Failed, ex);
                throw new IntegrationPointsException(ErrorMessages.Login_Failed, authException)
                {
                    ShouldAddToErrorsTab = true,
                    ExceptionSource = IntegrationPointsExceptionSource.IAPI
                };
            }
            catch (Exception ex)
            {
                LogCreatingImportApiError(ex, settings.WebServiceURL);
                throw;
            }
        }

        public IImportApiFacade GetImportApiFacade(ImportSettings settings)
        {
            return new ImportApiFacade(this, settings, _logger);
        }

        protected virtual IImportAPI CreateImportAPIForSettings(ImportSettings settings)
        {
            if (settings.FederatedInstanceArtifactId != null)
            {
                throw new NotSupportedException("Instance-to-instance import is not supported.");
            }

            return CreateImportAPI(settings.WebServiceURL);
        }

        protected virtual IImportAPI CreateImportAPI(string webServiceUrl)
        {
            IRelativityTokenProvider relativityTokenProvider = new RelativityTokenProvider(_authTokenGenerator);
            AppSettings.Instance.ImportBatchSize = _instanceSettingsManager.GetIApiBatchSize();
            return ExtendedImportAPI.CreateByTokenProvider(webServiceUrl, relativityTokenProvider);
        }

        private IImportAPI CreateImportAPI(ImportSettings settings)
        {
            IImportAPI importApi = CreateImportAPIForSettings(settings);
            var concreteImplementation = (Relativity.ImportAPI.ImportAPI)importApi;
            concreteImplementation.ExecutionSource = ExecutionSourceEnum.RIP;
            LogImportApiCreated();
            return importApi;
        }

        #region Logging

        private void LogImportSettings(ImportSettings importSettings)
        {
            var importSettingsForLogging = new ImportSettingsForLogging(importSettings);

            _logger.LogInformation("ImportSettings: {@importSettings}", importSettingsForLogging);
        }

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

        private class RelativityTokenProvider : IRelativityTokenProvider
        {
            private readonly IAuthTokenGenerator _authTokenGenerator;

            public RelativityTokenProvider(IAuthTokenGenerator authTokenGenerator)
            {
                _authTokenGenerator = authTokenGenerator;
            }

            public string GetToken()
            {
                return _authTokenGenerator.GetAuthToken();
            }
        }
    }
}
