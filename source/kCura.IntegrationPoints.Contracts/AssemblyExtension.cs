using System;
using System.Linq;
using System.Reflection;

namespace kCura.IntegrationPoints
{
	public static class AssemblyExtension
	{
		/// <summary>
		/// Get all loadable type from an assembly. This method will exclude private and internal classes
		/// </summary>
		/// <param name="assembly">the target assembly to retrieve types</param>
		/// <returns>an array of types from the given assembly</returns>
		public static Type[] GetLoadableTypes(this Assembly assembly)
		{
			Type[] loadableTypes = null;
			try
			{
				loadableTypes = assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException exception)
			{
				// some assemblies contain unloadable types via reflection - ie : class that inherits from internal interface/class
				loadableTypes = exception.Types.Where(t => t != null).ToArray();
			}
			return loadableTypes;
		}
	}
}