using System;
using System.Linq;
using System.Reflection;

namespace kCura.IntegrationPoints
{
	public static class AssmblyExtension
	{
		public static Type[] GetLoadableTypes(this Assembly assembly)
		{
			Type[] loadableTypes = null;
			try
			{
				loadableTypes = assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException exception)
			{
				// some assemblies contain unloadable types via reflection - ie : class that inherite from internal interface/class
				loadableTypes = exception.Types.Where(t => t != null).ToArray();
			}
			return loadableTypes;
		}
	}
}