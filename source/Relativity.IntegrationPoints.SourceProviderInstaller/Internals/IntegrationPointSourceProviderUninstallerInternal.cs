using System;
using System.Threading.Tasks;
using kCura.EventHandler;
using Relativity.IntegrationPoints.Services;

namespace Relativity.IntegrationPoints.SourceProviderInstaller.Internals
{
	internal class IntegrationPointSourceProviderUninstallerInternal
	{
		private const string _SUCCESS_MESSAGE = "Source Provider uninstalled successfully.";
		private const string _FAILURE_MESSAGE = "Uninstalling source provider failed.";

		private readonly IKeplerRequestHelper _keplerRequestHelper;
		private readonly Action _preExecuteAction;
		private readonly Action<bool, Exception> _postExecuteAction;

		public IntegrationPointSourceProviderUninstallerInternal(
			IKeplerRequestHelper keplerRequestHelper,
			Action preExecuteAction,
			Action<bool, Exception> postExecuteAction)
		{
			_keplerRequestHelper = keplerRequestHelper;
			_preExecuteAction = preExecuteAction;
			_postExecuteAction = postExecuteAction;
		}

		public Response Execute(int workspaceID, int applicationArtifactID)
		{
			Exception thrownException = null;

			Response responseToReturn = null;
			try
			{
				_preExecuteAction?.Invoke();
				UninstallProviderResponse uninstallProviderResponse = UninstallSourceProviderAsync(workspaceID, applicationArtifactID)
					.GetAwaiter()
					.GetResult();

				responseToReturn = new Response
				{
					Success = uninstallProviderResponse.Success,
					Message = uninstallProviderResponse.Success ? _SUCCESS_MESSAGE : uninstallProviderResponse.ErrorMessage
				};
			}
			catch (Exception ex)
			{
				thrownException = ex;
				responseToReturn = new Response
				{
					Success = false,
					Message = _FAILURE_MESSAGE,
					Exception = ex
				};
			}
			finally
			{
				bool isSuccess = responseToReturn?.Success ?? false;
				_postExecuteAction?.Invoke(isSuccess, thrownException);
			}

			return responseToReturn;
		}

		private Task<UninstallProviderResponse> UninstallSourceProviderAsync(int workspaceID, int applicationArtifactID)
		{
			var request = new UninstallProviderRequest
			{
				ApplicationID = applicationArtifactID,
				WorkspaceID = workspaceID
			};

			return SendUninstallProviderRequestWithRetriesAsync(request);
		}

		private Task<UninstallProviderResponse> SendUninstallProviderRequestWithRetriesAsync(UninstallProviderRequest request)
		{
			return _keplerRequestHelper
				.ExecuteWithRetriesAsync<IProviderManager, UninstallProviderRequest, UninstallProviderResponse>(
				(providerManager, r) => providerManager.UninstallProviderAsync(r),
				request
			);
		}
	}
}
