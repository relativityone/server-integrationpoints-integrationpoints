using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Castle.Facilities.TypedFactory;

namespace kCura.IntegrationPoints.Core.Services.Provider
{
	internal class DataProviderComponetSelector :DefaultTypedFactoryComponentSelector
	{
		protected override Type GetComponentType(MethodInfo method, object[] arguments)
		{
			if (method.Name.Equals("GetDataProvider"))
			{
				
			}
			return base.GetComponentType(method, arguments);
		}
	}
}
