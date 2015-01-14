using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace kCura.IntegrationPoints.SourceProviderInstaller
{
	public class ImportHelper
	{

		public static void ExtractEmbeddedResource(string outputDir, string resourceLocation, string file)
		{
			using (
				System.IO.Stream stream =
					System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceLocation + @"." + file))
			{
				using (
					System.IO.FileStream fileStream = new System.IO.FileStream(System.IO.Path.Combine(outputDir, file),
						System.IO.FileMode.Create))
				{
					for (int i = 0; i < stream.Length; i++)
					{
						fileStream.WriteByte((byte)stream.ReadByte());
					}
					fileStream.Close();
				}
			}
		}
	}
}