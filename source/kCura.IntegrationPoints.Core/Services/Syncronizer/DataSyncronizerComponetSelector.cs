using System;
using System.Linq;
using System.Reflection;
using Castle.Facilities.TypedFactory;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Core.Services.Syncronizer
{
	internal class DataSyncronizerComponetSelector : DefaultTypedFactoryComponentSelector
	{
		private static readonly string SyncProperty = typeof (IDataSyncronizerFactory).GetMethods().First().Name;

		protected override Type GetComponentType(MethodInfo method, object[] arguments)
		{
			if (method.Name.Equals(SyncProperty))
			{
				return typeof (RdoSynchronizer);
			}
			return base.GetComponentType(method, arguments);
		}
	}
}
