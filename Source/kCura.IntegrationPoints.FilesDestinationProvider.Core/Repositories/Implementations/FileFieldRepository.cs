using kCura.IntegrationPoints.Common.Constants;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using Relativity.Services.FileField;
using Relativity.Services.FileField.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Repositories.Implementations
{
	class FileFieldRepository : IFileFieldRepository
	{
		private IFileFieldManager _fileFieldManager;
		private IExternalServiceInstrumentationProvider _instrumentationProvider;

		public FileFieldRepository(IFileFieldManager fileFieldManager, IExternalServiceInstrumentationProvider instrumentationProvider)
		{
			_fileFieldManager = fileFieldManager;
			_instrumentationProvider = instrumentationProvider;
		}

		public DynamicFileResponse[] GetFilesForDynamicObjectsAsync(int workspaceID, int fileFieldArtifactID, int[] objectIDs)
		{
			IExternalServiceSimpleInstrumentation instrumentation =
				CreateInstrumentation(nameof(IFileFieldRepository.GetFilesForDynamicObjectsAsync));

			return instrumentation.Execute(() =>
				_fileFieldManager.GetFilesForDynamicObjectsAsync(workspaceID, fileFieldArtifactID, objectIDs)
					.GetAwaiter().GetResult());
		}
		private IExternalServiceSimpleInstrumentation CreateInstrumentation(string methodName)
		{
			return _instrumentationProvider.CreateSimple(
				ExternalServiceTypes.KEPLER,
				nameof(IFileFieldManager),
				methodName);
		}
	}
}
