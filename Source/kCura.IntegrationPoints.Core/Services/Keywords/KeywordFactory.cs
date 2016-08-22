using System.Collections.Generic;

namespace kCura.IntegrationPoints.Core.Services.Keywords
{
	public class KeywordFactory
	{
		private readonly IEnumerable<IKeyword> _keywords;
		public KeywordFactory(IEnumerable<IKeyword> keywords)
		{
			_keywords = keywords;
		}

		//REFACTOR TO DIFFERNT CLASS
		public IEnumerable<IKeyword> GetKeywords()
		{
			return _keywords;
		}
	}
}
