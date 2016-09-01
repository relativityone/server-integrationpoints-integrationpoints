using System;
using System.Web.Http;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoints.Web.Extensions
{
	public static class LogErrorExtensions
	{
		public static void HandleError(this ApiController apiController, IRSAPIClient context, IErrorRepository errorRepository, Exception ex, string userMessage = null)
		{
			ErrorDTO error = new ErrorDTO()
			{
				Message = userMessage??"Unexpected error occurred",
				FullText = $"{ex.Message}{Environment.NewLine}{ex.StackTrace}",
				Source = Core.Constants.IntegrationPoints.APPLICATION_NAME,
				WorkspaceId = context.APIOptions.WorkspaceID
			};
			errorRepository.Create(new[] { error });
		}
	}
}