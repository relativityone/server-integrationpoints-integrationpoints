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
		public void Load(string path)
		{
			ValidatePath(path);
			Assembly.Load(path);
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
			if (!System.IO.File.Exists(path)){
				throw new ArgumentException(String.Format("path \"{0}\" does not exist", path));
			}
		}

	}
}
