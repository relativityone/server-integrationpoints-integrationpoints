using System;
using System.Linq;
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
			var json = JsonConvert.DeserializeObject<ImportSettings>(options);
			var rdoObjectType = _query.GetObjectType(json.ArtifactTypeId);
			switch (rdoObjectType.Name.ToLower())
			{
//				case "custodian":
//					return _container.Kernel.Resolve<kCura.IntegrationPoints.Synchronizers.RDO.RDOCustodianSynchronizer>();
				default:
					return _container.Kernel.Resolve<kCura.IntegrationPoints.Synchronizers.RDO.RdoSynchronizer>();
			}
		}
	}
}
