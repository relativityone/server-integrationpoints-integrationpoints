using Autofac;

namespace Relativity.Sync
{
	/// <summary>
	///     Factory used to create <see cref="ISyncJob" />
	/// </summary>
	public interface ISyncJobFactory
	{
		/// <summary>
		///     Creates <see cref="ISyncJob" />
		/// </summary>
		/// <param name="container">Container initialized with all required adapters</param>
		/// <param name="syncJobParameters">Parameters of job to be created</param>
		/// <param name="relativityServices">Access to Relativity Services</param>
		ISyncJob Create(IContainer container, SyncJobParameters syncJobParameters, IRelativityServices relativityServices);

		/// <summary>
		///     Creates <see cref="ISyncJob" />
		/// </summary>
		/// <param name="container">Container initialized with all required adapters</param>
		/// <param name="syncJobParameters">Parameters of job to be created</param>
		/// <param name="relativityServices">Access to Relativity Services</param>
		/// <param name="logger">Logger</param>
		ISyncJob Create(IContainer container, SyncJobParameters syncJobParameters, IRelativityServices relativityServices, ISyncLog logger);

		/// <summary>
		///     Creates <see cref="ISyncJob" />
		/// </summary>
		/// <param name="container">Container initialized with all required adapters</param>
		/// <param name="syncJobParameters">Parameters of job to be created</param>
		/// <param name="relativityServices">Access to Relativity Services</param>
		/// <param name="configuration">Sync configuration</param>
		ISyncJob Create(IContainer container, SyncJobParameters syncJobParameters, IRelativityServices relativityServices, SyncJobExecutionConfiguration configuration);

		/// <summary>
		///     Creates <see cref="ISyncJob" />
		/// </summary>
		/// <param name="container">Container initialized with all required adapters</param>
		/// <param name="syncJobParameters">Parameters of job to be created</param>
		/// <param name="relativityServices">Access to Relativity Services</param>
		/// <param name="configuration">Sync configuration</param>
		/// <param name="logger">Logger</param>
		ISyncJob Create(IContainer container, SyncJobParameters syncJobParameters, IRelativityServices relativityServices, SyncJobExecutionConfiguration configuration, ISyncLog logger);
	}
}