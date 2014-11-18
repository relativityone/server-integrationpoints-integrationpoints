using System;
using System.Reflection;
using Castle.Facilities.TypedFactory;

namespace kCura.IntegrationPoints.Core.Services.Syncronizer
{
	internal class DataSyncronizerComponetSelector : DefaultTypedFactoryComponentSelector
	{
		protected override Type GetComponentType(MethodInfo method, object[] arguments)
		{
			if (method.Name.Equals("GetSyncronizer"))
			{

			}
			return base.GetComponentType(method, arguments);
		}
	}
}
