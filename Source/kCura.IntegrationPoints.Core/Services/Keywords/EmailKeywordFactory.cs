using System.Collections.Generic;

namespace kCura.IntegrationPoints.Core.Services.Keywords
{
	public class EmailKeywordFactory
	{
		private readonly IEnumerable<IKeyword> _keywords;
		public EmailKeywordFactory(IEnumerable<IKeyword> keywords)
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
