﻿using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Windsor;
using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Contracts.Synchronizer;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoints.Core.Services.Synchronizer
{
	public class ExportDestinationSynchronizerFactory : ISynchronizerFactory
	{
		private readonly IWindsorContainer _container;
		private readonly RSAPIRdoQuery _query;

		public ExportDestinationSynchronizerFactory(IWindsorContainer container, RSAPIRdoQuery query)
		{
			_container = container;
			_query = query;
		}

		public IDataSynchronizer CreateSynchronizer(Guid identifier, string options)
		{
			return _container.Kernel.Resolve<IDataSynchronizer>(typeof(ExportSynchroznizer).AssemblyQualifiedName);
		}
	}
}
