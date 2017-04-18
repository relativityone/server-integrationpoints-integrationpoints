using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data.Queries;

namespace kCura.IntegrationPoints.Core.Services
{
	public class ErrorService : IErrorService
	{
		private readonly CreateErrorRdo _createErrorRdo;

		public ErrorService(CreateErrorRdo createError)
		{
			_createErrorRdo = createError;
		}

		public virtual void Log(ErrorModel error)
		{
			var appVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
			var message = $"(Integration Points v({appVersion}) {error.Message}";
			_createErrorRdo.Execute(error.WorkspaceID, message, error.Exception);
		}
	}
}