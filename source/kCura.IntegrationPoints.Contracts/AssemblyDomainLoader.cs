using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Contracts
{
	/// <summary>
	/// Helper class used to load assemblies in the foreign app domain
	/// </summary>
	public class AssemblyDomainLoader : MarshalByRefObject
	{
		public AssemblyDomainLoader()
		{

		}
		/// <summary>
		/// Loads assembly the current app domain.
		/// </summary>
		/// <param name="rawAssembly">The library that will be loaded into the current Application Domain.</param>
		public void Load(byte[] rawAssembly)
		{
			if (rawAssembly == null)
			{
				throw new ArgumentNullException("rawAssembly");
			}
			Assembly.Load(rawAssembly);
		}

		private byte[] ReadFully(Stream stream)
		{
			byte[] buffer = new byte[16 * 1024];
			using (MemoryStream ms = new MemoryStream())
			{
				int read;
				while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
				{
					ms.Write(buffer, 0, read);
				}
				return ms.ToArray();
			}
		}

		public void Load(Stream assembyStream)
		{
			byte[] bytes = ReadFully(assembyStream);
			this.Load(bytes);
		}

		/// <summary>
		/// Loads assembly the current app domain.
		/// </summary>
		/// <param name="path">The library that will be loaded into the current Application Domain.</param>
		public void Load(string assemblyName)
		{
			Assembly.Load(assemblyName);
		}
		/// <summary>
		/// Loads assembly the current app domain.
		/// </summary>
		/// <param name="path">The library that will be loaded into the current Application Domain.</param>
		public void LoadFrom(string path)
		{
			ValidatePath(path);
			Assembly.LoadFrom(path);
		}

		private void ValidatePath(string path)
		{
			if (path == null) throw new ArgumentNullException("path");
			if (!System.IO.File.Exists(path))
			{
				throw new ArgumentException(String.Format("path \"{0}\" does not exist", path));
			}
		}

		private static Dictionary<String, Assembly> _assemblies = new Dictionary<String, Assembly>(StringComparer.OrdinalIgnoreCase);
		public static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
		{
			Assembly returnedAssembly = null;

			string dllName = new AssemblyName(args.Name).Name;
			string dllPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.PrivateBinPath, dllName + ".dll");

			lock (_assemblies)
			{
				if (_assemblies.Count == 0) GetLoadedAssemblies();
				if (!_assemblies.ContainsKey(dllName))
				{
					if (File.Exists(dllPath))
					{
						returnedAssembly = Assembly.LoadFile(dllPath);
					}
					_assemblies.Add(dllName, returnedAssembly);
				}
			}
			return _assemblies[dllName];
		}

		private static void GetLoadedAssemblies()
		{
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				_assemblies.Add(assembly.GetName().Name, assembly);
			}
		}

	}
}
