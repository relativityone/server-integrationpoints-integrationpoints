using kCura.IntegrationPoints.Common.Constants;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Data.Repositories.DTO;
using kCura.IntegrationPoints.Data.Repositories.Implementations.DTO;
using Relativity.Services.ResourceServer;
using System;
using kCura.IntegrationPoints.Domain.Exceptions;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
    internal class CaseRepository : ICaseRepository
    {
        private bool _isDisposed = false;
        private readonly IResourceServerManager _resourceServerService;
        private readonly IExternalServiceInstrumentationProvider _instrumentationProvider;

        /// <param name="resourceServerManager">It disposes resourceServerManager when Dispose is called</param>
        /// <param name="instrumentationProvider">Instrumentation provider</param>
        public CaseRepository(IResourceServerManager resourceServerManager, IExternalServiceInstrumentationProvider instrumentationProvider)
        {
            _resourceServerService = resourceServerManager;
            _instrumentationProvider = instrumentationProvider;
        }

        public ICaseInfoDto Read(int caseArtifactId)
        {
            CaseInfo caseInfo = ReadCaseInfo(caseArtifactId);
            return ConvertToDto(caseInfo);
        }

        private CaseInfo ReadCaseInfo(int caseArtifactId)
        {
            Func<CaseInfo> readCaseInfoFunction = () =>
                _resourceServerService.ReadCaseInfo(caseArtifactId).ConfigureAwait(false).GetAwaiter().GetResult();

            IExternalServiceSimpleInstrumentation simpleInstrumentation =
                _instrumentationProvider.CreateSimple(
                    ExternalServiceTypes.KEPLER, 
                    nameof(IResourceServerManager),
                    nameof(IResourceServerManager.ReadCaseInfo));
            try
            {
                return simpleInstrumentation.Execute(readCaseInfoFunction);
            }
            catch (Exception ex)
            {
                throw new IntegrationPointsException("Error occured while retrieving workspace details", ex)
                {
                    ExceptionSource = IntegrationPointsExceptionSource.KEPLER,
                    ShouldAddToErrorsTab = true
                };
            }
        }

        private ICaseInfoDto ConvertToDto(CaseInfo caseInfo)
        {
            return new CaseInfoDto(
                caseInfo.ArtifactID,
                caseInfo.Name,
                caseInfo.MatterArtifactID,
                caseInfo.StatusCodeArtifactID,
                caseInfo.EnableDataGrid,
                caseInfo.RootFolderID,
                caseInfo.RootArtifactID,
                caseInfo.DownloadHandlerURL,
                caseInfo.AsImportAllowed,
                caseInfo.ExportAllowed,
                caseInfo.DocumentPath
            );
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                _resourceServerService?.Dispose();
            }
            _isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
