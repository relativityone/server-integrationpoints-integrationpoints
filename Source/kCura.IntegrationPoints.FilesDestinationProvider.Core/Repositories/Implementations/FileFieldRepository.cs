using System.Runtime.CompilerServices;
using kCura.IntegrationPoints.Common.Constants;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using Relativity.Services.FileField;
using Relativity.Services.FileField.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Repositories.Implementations
{
	public class FileFieldRepository : IFileFieldRepository
	{
		private readonly IFileFieldManager _fileFieldManager;
		private readonly IExternalServiceInstrumentationProvider _instrumentationProvider;

		public FileFieldRepository(
			IFileFieldManager fileFieldManager, 
			IExternalServiceInstrumentationProvider instrumentationProvider)
		{
			_fileFieldManager = fileFieldManager;
			_instrumentationProvider = instrumentationProvider;
		}

		public DynamicFileResponse[] GetFilesForDynamicObjectsAsync(int workspaceID, int fileFieldArtifactID, int[] objectIDs)
		{
			return CreateInstrumentation().Execute(() =>
				_fileFieldManager.GetFilesForDynamicObjectsAsync(workspaceID, fileFieldArtifactID, objectIDs)
					.GetAwaiter()
					.GetResult()
			);
		}

		private IExternalServiceSimpleInstrumentation CreateInstrumentation([CallerMemberName]string methodName = "")
		{
			return _instrumentationProvider.CreateSimple(
				ExternalServiceTypes.KEPLER,
				nameof(IFileFieldManager),
				methodName);
		}
	}
}
