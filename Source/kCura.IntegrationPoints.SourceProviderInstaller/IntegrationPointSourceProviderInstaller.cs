﻿using kCura.EventHandler;
using kCura.IntegrationPoints.Contracts;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.SourceProviderInstaller.Internals;

namespace kCura.IntegrationPoints.SourceProviderInstaller
{
    /// <summary>
    /// Occurs immediately before the execution of a Post Install event handler.
    /// </summary>
    public delegate void PostInstallPreExecuteEvent();

    /// <summary>
    /// Occurs after all source providers are registered.
    /// </summary>
    /// <param name="isInstalled">Indicates whether the source providers were installed.</param>
    /// <param name="ex">An exception thrown when errors occur during the installation of a data source provider.</param>
    public delegate void PostInstallPostExecuteEvent(bool isInstalled, Exception ex);

    /// <summary>
    /// Registers the new data source providers with Relativity Integration Points.
    /// </summary>
    public abstract class IntegrationPointSourceProviderInstaller : PostInstallEventHandler
    {
        private const string _SUCCESS_MESSAGE = "Source Providers created or updated successfully.";

        private readonly Lazy<IAPILog> _logggerLazy;

        /// <summary>
        /// Raised immediately before the execution of a Post Install event handler.
        /// </summary>
        public event PostInstallPreExecuteEvent RaisePostInstallPreExecuteEvent;

        /// <summary>
        /// Raised after all source providers are registered.
        /// </summary>
        public event PostInstallPostExecuteEvent RaisePostInstallPostExecuteEvent;

        private IAPILog Logger => _logggerLazy.Value;

        /// <summary>
        /// Initializes <see cref="IntegrationPointSourceProviderInstaller"/>
        /// </summary>
        protected IntegrationPointSourceProviderInstaller()
        {
            _logggerLazy = new Lazy<IAPILog>(
                () => Helper.GetLoggerFactory().GetLogger().ForContext<IntegrationPointSourceProviderInstaller>()
            );
        }

        /// <summary>
        /// Retrieves the data source providers for registration with the application.
        /// </summary>
        /// <returns>The data source providers for registration.</returns>
        public abstract IDictionary<Guid, SourceProvider> GetSourceProviders();

        /// <inheritdoc cref="PostInstallEventHandler"/>
        public sealed override Response Execute()
        {
            IDictionary<Guid, SourceProvider> sourceProviders;
            try
            {
                sourceProviders = GetSourceProviders();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occured while getting source providers.");
                return new Response
                {
                    Success = false,
                    Message = "Error occured while getting source providers.",
                    Exception = ex,
                };
            }

            Logger.LogDebug("Starting Post-installation process for {sourceProviders} provider", sourceProviders.Values.Select(item => item.Name));

            if (sourceProviders.Count == 0)
            {
                return new Response
                {
                    Success = false,
                    Message = $"Provider does not implement the contract (Empty source provider list retrieved from {GetType().Name} class)"
                };
            }

            Exception thrownException = null;
            try
            {
                OnRaisePostInstallPreExecuteEvent();
                InstallSourceProvider(sourceProviders);

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
                OnRaisePostInstallPostExecuteEvent(isSuccess, thrownException);
            }
        }

        /// <summary>
        /// Raises an event prior to the execution of a Post Install event handler.
        /// </summary>
        protected void OnRaisePostInstallPreExecuteEvent()
        {
            RaisePostInstallPreExecuteEvent?.Invoke();
        }

        /// <summary>
        /// Occurs after the registration process completes.
        /// </summary>
        /// <param name="isInstalled">Indicates whether the data source providers were installed.</param>
        /// <param name="ex">An exception thrown when errors occur during the installation of the data source provider.</param>
        protected void OnRaisePostInstallPostExecuteEvent(bool isInstalled, Exception ex)
        {
            RaisePostInstallPostExecuteEvent?.Invoke(isInstalled, ex);
        }

        private void InstallSourceProvider(IDictionary<Guid, SourceProvider> providers)
        {
            if (!providers.Any())
            {
                throw new InvalidSourceProviderException("No Source Providers passed.");
            }

            IEnumerable<SourceProvider> sourceProviders = providers.Select(x => new SourceProvider
            {
                GUID = x.Key,
                ApplicationID = base.ApplicationArtifactId,
                ApplicationGUID = x.Value.ApplicationGUID,
                Name = x.Value.Name,
                Url = x.Value.Url,
                ViewDataUrl = x.Value.ViewDataUrl,
                Configuration = x.Value.Configuration
            });

            InstallSourceProviders(sourceProviders).GetAwaiter().GetResult();
        }

        internal virtual Task InstallSourceProviders(IEnumerable<SourceProvider> sourceProviders)
        {
            IServicesMgr servicesManager = Helper.GetServicesManager();
            var keplerSourceProviderInstaller = new KeplerSourceProviderInstaller(Logger, servicesManager);
            return keplerSourceProviderInstaller.InstallSourceProviders(Helper.GetActiveCaseID(), sourceProviders);
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