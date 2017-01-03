using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;


//Guidelines for class from: https://msdn.microsoft.com/en-us/library/9eekhta0(v=vs.110).aspx

namespace kCura.IntegrationPoints.ImportProvider.Parser
{
	public class EnumerableParser : IEnumerable<string[]>
	{
		private IEnumerable<string> _sourceFileLines;
		private char _columnDelimiter;
		private char _quoteDelimiter;

		public EnumerableParser(IEnumerable<string> sourceFileLines, char columnDelimiter, char quoteDelimiter)
		{
			_sourceFileLines = sourceFileLines;
			_columnDelimiter = columnDelimiter;
			_quoteDelimiter = quoteDelimiter;
		}

		public IEnumerator<string[]> GetEnumerator()
		{
			return new EnumerableParserEnumerator(_sourceFileLines, _columnDelimiter, _quoteDelimiter);
		}

		private IEnumerator GetEnumerator1()
		{
			return this.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator1();
		}
	}

	public class EnumerableParserEnumerator : IEnumerator<string[]>
	{
		private IEnumerable<string> _sourceFileLines;
		private IEnumerator<string> _sourceFileEnumerator;
		private char _columnDelimiter;
		private char _quoteDelimiter;
		private string _regex;
		private List<string> _current;
		private bool _hasNext;

		public EnumerableParserEnumerator(IEnumerable<string> sourceFileLines, char columnDelimiter, char quoteDelimiter)
		{
			_sourceFileLines = sourceFileLines;
			_columnDelimiter = columnDelimiter;
			_quoteDelimiter = quoteDelimiter;
			_hasNext = true;
			_regex = BuildRegex(columnDelimiter, quoteDelimiter);
			ResetEnumerator();
		}

		~EnumerableParserEnumerator()
		{
			this.Dispose();
		}

		public string[] Current
		{
			get { return _current.ToArray(); }
		}

		private object Current1
		{
			get { return this.Current; }
		}

		object IEnumerator.Current
		{
			get { return Current1; }
		}

		public bool MoveNext()
		{
			bool rv = _hasNext;
			if (rv)
			{
				_current = (from Match m in Regex.Matches(_sourceFileEnumerator.Current, _regex)
							select m.ToString().Trim(_quoteDelimiter)).ToList();
				_hasNext = _sourceFileEnumerator.MoveNext();
			}
			return rv;
		}

		public void Reset()
		{
			ResetEnumerator();
		}

		public void Dispose()
		{
			_sourceFileEnumerator.Dispose();
		}

		private void ResetEnumerator()
		{
			_sourceFileEnumerator = _sourceFileLines.GetEnumerator();
			_hasNext = _sourceFileEnumerator.MoveNext();
		}

		private string BuildRegex(char columnDelimiter, char quoteDelimiter)
		{
			//Example regex for:
			//	quote delimiter:   "   ==> ASCII 34 (0x22)
			//	column delimiter:  ,   ==> ASCII 44 (0x2c)
			//	[\x22].+?[\x22]|[^\x2c]+

			string quoteHex = ((int)quoteDelimiter).ToString("x2");
			string colHex = ((int)columnDelimiter).ToString("x2");

			return string.Concat(new string[] {
				@"[\x",
				quoteHex,
				@"].+?[\x",
				quoteHex,
				@"]|[^",
				@"\x",
				colHex,
				@"]+"
			});
		}
	}
}