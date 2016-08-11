using System.Collections.Generic;
using kCura.WinEDDS;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Process
{
	internal class ExportFileHelper
	{
		public static void SetDefaultValues(ExportFile expFile)
		{
			expFile.AppendOriginalFileName = false;
			expFile.ExportNativesToFileNamedFrom = ExportNativeWithFilenameFrom.Identifier;
			var imagePrecs = new List<Pair>();
			imagePrecs.Add(new Pair("-1", "Original"));
			expFile.ImagePrecedence = imagePrecs.ToArray();
			expFile.ObjectTypeName = "Document";
			expFile.RenameFilesToIdentifier = true;
			expFile.TypeOfExport = ExportFile.ExportType.ArtifactSearch;
			expFile.ViewID = 0;
		}
	}
}