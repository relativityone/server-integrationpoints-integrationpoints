using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using kCura.EventHandler;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts;

namespace Relativity.IntegrationPoints.SourceProviderInstaller.Internals
{
	internal class IntegrationPointSourceProviderInstallerInternal
	{
		private const string _SUCCESS_MESSAGE = "Source Providers created or updated successfully.";

		private readonly IAPILog _logger;
		private readonly ISourceProviderInstaller _sourceProviderInstaller;

		private readonly Func<IDictionary<Guid, SourceProvider>> _sourceProvidersProvider;
		private readonly Action _preExecuteAction;
		private readonly Action<bool, Exception> _postExecuteAction;

		public IntegrationPointSourceProviderInstallerInternal(
			IAPILog logger,
			ISourceProviderInstaller sourceProviderInstaller,
			Func<IDictionary<Guid, SourceProvider>> sourceProvidersProvider,
			Action preExecuteAction,
			Action<bool, Exception> postExecuteAction)
		{
			_logger = logger.ForContext<IntegrationPointSourceProviderInstallerInternal>();
			_sourceProviderInstaller = sourceProviderInstaller;
			_sourceProvidersProvider = sourceProvidersProvider;
			_preExecuteAction = preExecuteAction;
			_postExecuteAction = postExecuteAction;
		}

		public Response Execute(int workspaceID, int applicationArtifactID)
		{
			IDictionary<Guid, SourceProvider> sourceProviders;
			try
			{
				sourceProviders = _sourceProvidersProvider();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occured while getting source providers.");
				return new Response
				{
					Success = false,
					Message = "Error occured while getting source providers.",
					Exception = ex,
				};
			}

			_logger.LogDebug("Starting Post-installation process for {sourceProviders} provider", sourceProviders.Values.Select(item => item.Name));

			if (sourceProviders.Count == 0)
			{
				return new Response
				{
					Success = false,
					Message = $"Provider does not implement the contract (Empty source provider list returned by {nameof(IntegrationPointSourceProviderInstaller.GetSourceProviders)})"
				};
			}

			Exception thrownException = null;
			try
			{
				_preExecuteAction?.Invoke();
				InstallSourceProvider(workspaceID, applicationArtifactID, sourceProviders);

				return new Response
				{
					Success = true,
					Message = _SUCCESS_MESSAGE
				};
			}
			catch (Exception ex)
			{
				thrownException = ex;
				return new Response
				{
					Success = false,
					Message = GetFailureMessage(sourceProviders),
					Exception = ex
				};
			}
			finally
			{
				bool isSuccess = thrownException == null;
				_postExecuteAction?.Invoke(isSuccess, thrownException);
			}
		}

		private void InstallSourceProvider(
			int workspaceID,
			int applicationArtifactID,
			IDictionary<Guid, SourceProvider> providers)
		{
			if (!providers.Any())
			{
				throw new InvalidSourceProviderException("No Source Providers passed.");
			}

			IEnumerable<SourceProvider> sourceProviders = providers.Select(x => new SourceProvider
			{
				GUID = x.Key,
				ApplicationID = applicationArtifactID,
				ApplicationGUID = x.Value.ApplicationGUID,
				Name = x.Value.Name,
				Url = x.Value.Url,
				ViewDataUrl = x.Value.ViewDataUrl,
				Configuration = x.Value.Configuration
			});

			_sourceProviderInstaller.InstallSourceProvidersAsync(workspaceID, sourceProviders).GetAwaiter().GetResult();
		}

		private static string GetFailureMessage(IDictionary<Guid, SourceProvider> sourceProviders)
		{
			var failureMessage = new StringBuilder("Failed to install");
			if (sourceProviders != null)
			{
				foreach (SourceProvider sourceProvider in sourceProviders.Values)
				{
					failureMessage.Append(" [Provider: ");
					failureMessage.Append(sourceProvider?.Name);
					failureMessage.Append("]");
				}
			}

			return failureMessage.ToString();
		}
	}
}
