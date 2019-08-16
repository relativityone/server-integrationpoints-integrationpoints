using System.Collections.Generic;

namespace kCura.IntegrationPoints.Core.Services.Exporter.Sanitization
{
	internal interface IChoiceTreeToStringConverter
	{
		string ConvertTreeToString(IList<ChoiceWithChildInfo> tree);
	}
}
