using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.Relativity.Client;
using NUnit.Framework;

namespace JsonLoader.Tests.Integration
{
	[TestFixture]
	public class LibraryLoader
	{
		[Test]
		[Explicit]
		public void Load()
		{
			string libLocation = @"C:\\t.txt";
			var client = new RSAPIClient(new Uri("http://localhost/Relativity.Services"), new IntegratedAuthCredentials());
			client.APIOptions.WorkspaceID = 1025258;
			
			var request = new UploadRequest(client.APIOptions);
			request.Target.FieldId = 1037609;
			request.Target.ObjectArtifactId = 1037493;
			request.Metadata.FileName = libLocation;
			request.Metadata.FileSize = new FileInfo(libLocation).Length;
			request.Overwrite = true;
			client.Upload(request);
		}
	}
}
