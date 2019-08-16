using System.Collections.Generic;

namespace kCura.IntegrationPoints.Core.Services.Exporter.Sanitization
{
	internal interface IExportFieldSanitizerProvider
	{
		IList<IExportFieldSanitizer> GetExportFieldSanitizers();
	}
}
