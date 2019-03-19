﻿using System;
using Autofac;
using Banzai.Logging;
using Relativity.Sync.Logging;

namespace Relativity.Sync
{
	/// <inheritdoc />
	public sealed class SyncJobFactory : ISyncJobFactory
	{
		/// <inheritdoc />
		public ISyncJob Create(IContainer container, SyncJobParameters syncJobParameters)
		{
			return Create(container, syncJobParameters, new SyncConfiguration(), new EmptyLogger());
		}

		/// <inheritdoc />
		public ISyncJob Create(IContainer container, SyncJobParameters syncJobParameters, ISyncLog logger)
		{
			return Create(container, syncJobParameters, new SyncConfiguration(), logger);
		}

		/// <inheritdoc />
		public ISyncJob Create(IContainer container, SyncJobParameters syncJobParameters, SyncConfiguration configuration)
		{
			return Create(container, syncJobParameters, configuration, new EmptyLogger());
		}

		/// <inheritdoc />
		public ISyncJob Create(IContainer container, SyncJobParameters syncJobParameters, SyncConfiguration configuration, ISyncLog logger)
		{
			if (container == null)
			{
				throw new ArgumentNullException(nameof(container));
			}

			if (syncJobParameters == null)
			{
				throw new ArgumentNullException(nameof(syncJobParameters));
			}

			if (configuration == null)
			{
				throw new ArgumentNullException(nameof(configuration));
			}

			if (logger == null)
			{
				throw new ArgumentNullException(nameof(logger));
			}

			try
			{
				LogWriter.SetFactory(new SyncLogWriterFactory(logger));

				IContainerFactory containerFactory = new ContainerFactory(syncJobParameters, configuration, logger);

				using (ILifetimeScope scope = container.BeginLifetimeScope(builder => containerFactory.RegisterSyncDependencies(builder)))
				{
					return scope.Resolve<ISyncJob>();
				}
			}
			catch (Exception e)
			{
				logger.LogError(e, "Failed to create Sync job {correlationId}.", syncJobParameters.CorrelationId);
				throw new SyncException("Unable to create Sync job. See inner exception for more details.", e, syncJobParameters.CorrelationId);
			}
		}
	}
}