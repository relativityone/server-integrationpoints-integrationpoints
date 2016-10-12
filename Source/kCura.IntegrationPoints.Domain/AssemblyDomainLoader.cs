using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace kCura.IntegrationPoints.Contracts.Domain
{
	/// <summary>
	/// Helper class used to load assemblies in the foreign app domain
	/// </summary>
	internal class AssemblyDomainLoader : MarshalByRefObject
	{
		public AssemblyDomainLoader()
		{
		}

		public readonly HashSet<string> MergedBinariesList = new HashSet<string>()
		{
			"kCura.IntegrationPoints.Data",
			"kCura.Apps.Common.Config",
			"kCura.Apps.Common.Data",
			"kCura.Apps.Common.Utils",
			"kCura.IntegrationPoints.Injection",
			"kCura.Injection",
			"kCura.IntegrationPoints.Core.Contracts",
			"kCura.ScheduleQueue.AgentBase",
			"kCura.IntegrationPoints.Config",
			"kCura.IntegrationPoints.Core",
			"kCura.IntegrationPoints.SourceProviderInstaller",
			"kCura.IntegrationPoints.Synchronizers.RDO",
			"kCura.IntegrationPoints.EventHandlers",
			"kCura.IntegrationPoints.Agent",
			"kCura.IntegrationPoints.Agent",
			"kCura.IntegrationPoints.Email",
			"kCura.IntegrationPoints.DocumentTransferProvider.Shared",
			"kCura.ScheduleQueue.Core"
		};

		/// <summary>
		/// Loads assembly the current app domain.
		/// </summary>
		/// <param name = "rawAssembly" > The library that will be loaded into the current Application Domain.</param>
		public void Load(byte[] rawAssembly)
		{
			if (rawAssembly == null)
			{
				throw new ArgumentNullException("rawAssembly");
			}
			System.Reflection.Assembly.Load(rawAssembly);
		}

		private byte[] ReadFully(Stream stream)
		{
			byte[] buffer = new byte[16*1024];
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
		/// <param name = "assemblyName" > The library that will be loaded into the current Application Domain.</param>
		public void Load(string assemblyName)
		{
			System.Reflection.Assembly.Load(assemblyName);
		}

		/// <summary>
		/// Loads assembly the current app domain.
		/// </summary>
		/// <param name = "path" > The library that will be loaded into the current Application Domain.</param>
		public void LoadFrom(string path)
		{
			ValidatePath(path);
			System.Reflection.Assembly.LoadFrom(path);
		}

		private void ValidatePath(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			if (!File.Exists(path))
			{
				throw new ArgumentException($"Path \"{path}\" does not exist");
			}
		}

		private Dictionary<String, System.Reflection.Assembly> _assemblies =
			new Dictionary<String, System.Reflection.Assembly>(StringComparer.OrdinalIgnoreCase);

		public System.Reflection.Assembly ResolveAssembly(object sender, ResolveEventArgs args)
		{
			System.Reflection.Assembly returnedAssembly = null;

			string dllName = new AssemblyName(args.Name).Name;

			returnedAssembly = ResolveAssemblyInDirectory(dllName, AppDomain.CurrentDomain.SetupInformation.ApplicationBase);
			if (returnedAssembly == null)
			{
				returnedAssembly = ResolveAssemblyInDirectory(dllName, AppDomain.CurrentDomain.SetupInformation.PrivateBinPath);
			}
			return returnedAssembly;
		}

		public System.Reflection.Assembly ResolveAssemblyInDirectory(string dllName, string searchDirectory)
		{
			System.Reflection.Assembly returnedAssembly = null;

			string dllPath = Path.Combine(searchDirectory, dllName + ".dll");

			lock (_assemblies)
			{
				if (_assemblies.Count == 0)
				{
					GetLoadedAssemblies();
				}
				if (!_assemblies.ContainsKey(dllName))
				{
					if (File.Exists(dllPath))
					{
						returnedAssembly = System.Reflection.Assembly.LoadFile(dllPath);
					}
					else
					{
						returnedAssembly = ResolveMergedAssembly(dllName, searchDirectory);
					}
					if (returnedAssembly != null)
					{
						_assemblies.Add(dllName, returnedAssembly);
					}
				}
				else
				{
					returnedAssembly = _assemblies[dllName];
				}
			}
			return returnedAssembly;
		}


		public System.Reflection.Assembly ResolveMergedAssembly(string dllName, string searchDirectory)
		{
			System.Reflection.Assembly returnedAssembly = null;
			if (MergedBinariesList.Any(s => { return s.Equals(dllName, StringComparison.InvariantCultureIgnoreCase); }))
			{
				string dllPath = Path.Combine(searchDirectory, "kCura.IntegrationPoints.dll");
				if (File.Exists(dllPath))
				{
					returnedAssembly = System.Reflection.Assembly.LoadFile(dllPath);
				}
			}
			return returnedAssembly;
		}

		public string ResolveMergedAssemblyFullName(string dllName, string searchDirectory)
		{
			string mergedAssemblyFullName = null;
			System.Reflection.Assembly mergedAssembly = ResolveAssemblyInDirectory(dllName, searchDirectory);
			if (mergedAssembly != null)
			{
				mergedAssemblyFullName = mergedAssembly.FullName;
			}
			return mergedAssemblyFullName;
		}

		private void GetLoadedAssemblies()
		{
			foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				_assemblies.Add(assembly.GetName().Name, assembly);
			}
		}
	}
}