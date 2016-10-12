using System.Reflection;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data.Queries;

namespace kCura.IntegrationPoints.Core.Services
{
	public interface IErrorService
	{
		void Log(ErrorModel error);
	}

	public class ErrorService : IErrorService
	{
		private readonly CreateErrorRdo _createErrorRdo;

		public ErrorService(CreateErrorRdo createError)
		{
			_createErrorRdo = createError;
		}

		public virtual void Log(ErrorModel error)
		{
			try
			{
				var appVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
				var message = string.Format("(Integration Points v({0}) {1}", appVersion, error.Message);
				_createErrorRdo.Execute(error.WorkspaceID, message, error.Exception);
			}
			catch
			{
				//Eat me
			}
		}
	}
}