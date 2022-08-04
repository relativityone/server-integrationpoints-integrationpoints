using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.Keywords
{
    public class EmailFormatter : IEmailFormatter
    {
        private readonly IEnumerable<IKeyword> _keywords;
        private readonly IAPILog _logger;

        public EmailFormatter(IHelper helper, IEnumerable<IKeyword> keywords)
        {
            _keywords = keywords;
            _logger = helper.GetLoggerFactory().GetLogger().ForContext<EmailFormatter>();
        }

        public string Format(string textToFormat)
        {
            string returnValue = textToFormat;
            if (string.IsNullOrEmpty(returnValue))
            {
                return returnValue;
            }
            var dictionary = _keywords.ToDictionary(x => x.KeywordName.ToUpperInvariant());
            var matchPattern = string.Join("|", dictionary.Keys);
            var expression = new Regex(matchPattern, RegexOptions.IgnoreCase);
            var matches = expression.Matches(textToFormat);
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
                    LogFormattingError(e);
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

        private void LogFormattingError(Exception e)
        {
            _logger.LogError(e, "Error occurred while formatting email text");
        }

        #endregion
    }
}