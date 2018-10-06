using System;
using System.Linq;
using System.Text.RegularExpressions;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.Keywords
{
	public class KeywordConverter
	{
		private readonly KeywordFactory _factory;
		private readonly IAPILog _logger;

		public KeywordConverter(IHelper helper, KeywordFactory factory)
		{
			_factory = factory;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<KeywordConverter>();
		}

		public string Convert(string textToConvert)
		{
			string returnValue = textToConvert;
			if (string.IsNullOrEmpty(returnValue))
			{
				return returnValue;
			}
			var dictionary = _factory.GetKeywords().ToDictionary(x => x.KeywordName.ToUpperInvariant());
			var matchPattern = string.Join("|", dictionary.Keys);
			var expression = new Regex(matchPattern, RegexOptions.IgnoreCase);
			var matches = expression.Matches(textToConvert);
			//only replace the keys found
			foreach (Match match in matches)
			{
				IKeyword keyword;
				if (!dictionary.TryGetValue(@"\" + match.Value.ToUpperInvariant(), out keyword))
				{
					continue;
				}
				string replacementValue = string.Empty;
				try
				{
					replacementValue = keyword.Convert();
				}
				catch (Exception e)
				{
					LogConvertingError(textToConvert, e);
					//eat
				}
				if (!string.IsNullOrEmpty(replacementValue))
				{
					returnValue = Regex.Replace(returnValue, @"\" + match.Value, replacementValue);
				}
				else
				{
					//remove line if this keyword is the only entry on the line
					returnValue = Regex.Replace(returnValue, Environment.NewLine + @"\" + match.Value + Environment.NewLine, replacementValue);
					//remove keyword for cases when this keyword is not the only entry on the line
					returnValue = Regex.Replace(returnValue, @"\" + match.Value, replacementValue);
				}
			}
			return returnValue;
		}

		#region Logging

		private void LogConvertingError(string textToConvert, Exception e)
		{
			_logger.LogError(e, "Error occurred during text convertion ({TextToConvert})", textToConvert);
		}

		#endregion
	}
}