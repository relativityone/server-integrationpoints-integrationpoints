using System;
using System.Reflection;
using kCura.IntegrationPoints.Core.Models;

namespace kCura.IntegrationPoints.Core.Services
{
	public class CreateError
	{
		private readonly Data.Queries.CreateErrorRdo _createErrorRdo;
		public CreateError(Data.Queries.CreateErrorRdo createError)
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
