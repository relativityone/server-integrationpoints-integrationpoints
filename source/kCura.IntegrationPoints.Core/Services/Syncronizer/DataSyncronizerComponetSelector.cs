using System;
using System.Reflection;
using Castle.Facilities.TypedFactory;
using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoints.Core.Services.Syncronizer
{
	internal class DataSyncronizerComponetSelector : DefaultTypedFactoryComponentSelector
	{
		protected override Type GetComponentType(MethodInfo method, object[] arguments)
		{
			if (method.Name.Equals("GetSyncronizer"))
			{
				return typeof (RdoSynchronizer);
			}
			return base.GetComponentType(method, arguments);
		}
	}
}
