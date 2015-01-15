﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public class EmbeddedAssembly
	{
		static Dictionary<string, Assembly> dic = null;
		static Dictionary<string, string> dic2 = null;

		public static void Load(string embeddedResource, string fileName)
		{
			if (dic == null)
				dic = new Dictionary<string, Assembly>();
			if (dic2 == null)
				dic2 = new Dictionary<string, string>();

			if (dic2.ContainsKey(embeddedResource)) return;

			byte[] ba = null;
			Assembly asm = null;
			Assembly curAsm = Assembly.GetExecutingAssembly();

			using (Stream stm = curAsm.GetManifestResourceStream(embeddedResource))
			{
				// Either the file is not existed or it is not mark as embedded resource
				if (stm == null)
					throw new Exception(embeddedResource + " is not found in Embedded Resources.");

				// Get byte[] from the file from embedded resource
				ba = new byte[(int)stm.Length];
				stm.Read(ba, 0, (int)stm.Length);
				try
				{
					asm = Assembly.Load(ba);

					// Add the assembly/dll into dictionary
					dic.Add(asm.FullName, asm);
					dic2.Add(embeddedResource, asm.FullName);
					return;
				}
				catch
				{
					// Purposely do nothing
					// Unmanaged dll or assembly cannot be loaded directly from byte[]
					// Let the process fall through for next part
				}
			}

			bool fileOk = false;
			string tempFile = "";

			using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider())
			{
				string fileHash = BitConverter.ToString(sha1.ComputeHash(ba)).Replace("-", string.Empty); ;

				tempFile = Path.GetTempPath() + fileName;

				if (File.Exists(tempFile))
				{
					byte[] bb = File.ReadAllBytes(tempFile);
					string fileHash2 = BitConverter.ToString(sha1.ComputeHash(bb)).Replace("-", string.Empty);

					if (fileHash == fileHash2)
					{
						fileOk = true;
					}
					else
					{
						fileOk = false;
					}
				}
				else
				{
					fileOk = false;
				}
			}

			if (!fileOk)
			{
				System.IO.File.WriteAllBytes(tempFile, ba);
			}

			asm = Assembly.LoadFile(tempFile);
			if(dic.ContainsKey(asm.FullName)){}
			dic.Add(asm.FullName, asm);
		}

		public static Assembly Get(string assemblyFullName)
		{
			if (dic == null || dic.Count == 0)
				return null;

			if (dic.ContainsKey(assemblyFullName))
				return dic[assemblyFullName];

			return null;
		}
	}
}
