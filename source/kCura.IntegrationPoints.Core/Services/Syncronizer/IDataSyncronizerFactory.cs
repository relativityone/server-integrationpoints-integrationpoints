using System;
using Castle.Windsor;
using kCura.IntegrationPoints.Contracts.Syncronizer;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Newtonsoft.Json;
using Component = Castle.MicroKernel.Registration.Component;

namespace kCura.IntegrationPoints.Core.Services.Syncronizer
{
	public interface IDataSyncronizerFactory
	{
		IDataSyncronizer GetSyncronizer(Guid identifier, string options);
	}

	public class MockFactory : IDataSyncronizerFactory
	{
		private readonly IWindsorContainer _container;
		private readonly RelativityRdoQuery _query;
		public MockFactory(IWindsorContainer container, RelativityRdoQuery query)
		{
			_container = container;
			_query = query;
		}

		public IDataSyncronizer GetSyncronizer(Guid identifier, string options)
		{
			return kCura.IntegrationPoints.Contracts.PluginBuilder.Current.GetSynchronizerFactory()
				.CreateSyncronizer(identifier, options);
		}
	}
}
