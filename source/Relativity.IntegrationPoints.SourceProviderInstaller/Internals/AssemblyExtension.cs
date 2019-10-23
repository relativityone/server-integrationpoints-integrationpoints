using System;
using System.Linq;
using System.Reflection;

namespace Relativity.IntegrationPoints.SourceProviderInstaller.Internals
{
	/// <summary>
	/// A helper class reponsible for loading assemblies and their types.
	/// </summary>
	internal static class AssemblyExtension
	{
		/// <summary>
		/// Retrieves all public types from a given assembly. This method will exclude private and internal classes.
		/// </summary>
		/// <param name="assembly">The target assembly to retrieve types from.</param>
		/// <returns>Returns an array of types from the given assembly.</returns>
		public static Type[] GetLoadableTypes(this System.Reflection.Assembly assembly)
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