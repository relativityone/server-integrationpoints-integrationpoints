using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace kCura.IntegrationPoints.Core.Services.Keywords
{
  public class KeywordConverter
  {
    private readonly KeywordFactory _factory;
    public KeywordConverter(KeywordFactory factory)
    {
      _factory = factory;
    }

    public string Convert(string textToConvert)
    {
      string returnValue = textToConvert;
      if (string.IsNullOrEmpty(returnValue))
      {
        return returnValue;
      }
      var dictionary = _factory.GetKeywords().ToDictionary(x => x.KeywordName.ToUpper());
      var matchPattern = string.Join("|", dictionary.Keys);
      var expression = new Regex(matchPattern, RegexOptions.IgnoreCase);
      var matches = expression.Matches(textToConvert);
      //only replace the keys found
      foreach (Match match in matches)
      {
        IKeyword keyword;
        if (!dictionary.TryGetValue(@"\" + match.Value.ToUpper(), out keyword))
        {
          continue;
        }
        string replacementValue = string.Empty;
        try
        {
          replacementValue = keyword.Convert();
        }
        catch (Exception)
        {
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



  }
}
