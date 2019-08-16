using System.Collections.Generic;
using kCura.Apps.Common.Utils.Serializers;

namespace kCura.IntegrationPoints.Core.Services.Exporter.Sanitization
{
	internal class ExportFieldSanitizerProvider : IExportFieldSanitizerProvider
	{
		private readonly ISerializer _serializer;
		private readonly IChoiceCache _choiceCache;
		private readonly IChoiceTreeToStringConverter _choiceTreeConverter;

		public ExportFieldSanitizerProvider(
			ISerializer serializer, 
			IChoiceCache choiceCache,
			IChoiceTreeToStringConverter choiceTreeConverter)
		{
			_serializer = serializer;
			_choiceCache = choiceCache;
			_choiceTreeConverter = choiceTreeConverter;
		}

		public IList<IExportFieldSanitizer> GetExportFieldSanitizers()
		{
			IList<IExportFieldSanitizer> sanitizers = new List<IExportFieldSanitizer>
			{
				new SingleObjectFieldSanitizer(_serializer),
				new MultipleObjectFieldSanitizer(_serializer),
				new SingleChoiceFieldSanitizer(_serializer),
				new MultipleChoiceFieldSanitizer(_choiceCache, _choiceTreeConverter, _serializer)
			};

			return sanitizers;
		}
	}
}
