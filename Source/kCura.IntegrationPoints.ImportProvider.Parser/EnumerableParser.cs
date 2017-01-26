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
		private string[] _current;
		private bool _hasNext;
		private string _regexPattern;
		private string _columnDelimiterString;
		private string _quoteDelimiterString;
		private string _doubleColumnDelimiterString;
		private string _doubleQuoteDelimiterString;

		public EnumerableParserEnumerator(IEnumerable<string> sourceFileLines, char columnDelimiter, char quoteDelimiter)
		{
			_sourceFileLines = sourceFileLines;
			_columnDelimiter = columnDelimiter;
			_quoteDelimiter = quoteDelimiter;

			_columnDelimiterString = columnDelimiter.ToString();
			_quoteDelimiterString = quoteDelimiter.ToString();
			_doubleQuoteDelimiterString = new string(quoteDelimiter, 2);
			_doubleColumnDelimiterString = new string(columnDelimiter, 2);

			_regexPattern = BuildRegex(columnDelimiter, quoteDelimiter);

			_hasNext = true;
			ResetEnumerator();
		}

		~EnumerableParserEnumerator()
		{
			this.Dispose();
		}

		public string[] Current
		{
			get { return _current; }
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
				UpdateCurrent();
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

		private void UpdateCurrent()
		{
			_current = (from Match m
						in Regex.Matches(_sourceFileEnumerator.Current, _regexPattern)
						select TransformArtifact(m.Value))
						.ToArray();
		}

		private string TransformArtifact(string value)
		{
			return value
				.Remove(value.Length - 1, 1) //strip end quote
				.Remove(0, 1) //strip lead quote
				.Replace(_doubleQuoteDelimiterString, _quoteDelimiterString)
				.Replace(_doubleColumnDelimiterString, _columnDelimiterString);

		}

		private string BuildRegex(char columnDelimiter, char quoteDelimiter)
		{
			//	Example character classes:
			//	quote delimiter:    "   ==> ASCII 34 (0x22)
			//	column delimiter:   ,   ==> ASCII 44 (0x2c)

			//	Example regex: 
			//	[\x22].*?[\x22](?=($)$|[\x2c][\x22])

			//	Regex breakdown:
			//	[\x22].*?[\x22]
			//			Match string delimited by quote delimiters, zero or more characters in between, lazy (i.e. non-greedy) match
			//	(?=
			//			Start a zero-width lookahead
			//	($)
			//			Alternation test: Is the next character end of line?
			//	$
			//			Alternation "then"; If so match end of line as part of this lookahead; (this matches the last column)
			//	|
			//			Alternation "else":
			//	[\x2c][\x22]
			//			Match column delimiter followed by quote delimiter (start of next column). This pattern can only occur outside of a data column in the transformed string
			//	)
			//			Close lookahead
			//
			//	Motivation: because LoadFileDataReader doubles up all quote and column delimiters in each source ArtifactField, the following string can only occur
			//	at the boundary of artifacts (i.e. string join on column delimiter):
			//		","

			string columnCharacterClass = HexCharClass(columnDelimiter);
			string quoteCharacterClass = HexCharClass(quoteDelimiter);

			return string.Concat(new string[] {
				quoteCharacterClass,
				".*?",
				quoteCharacterClass,
				"(?=($)$|",
				columnCharacterClass,
				quoteCharacterClass,
				")"
			});
		}

		private static string HexCharClass(char delim)
		{
			return string.Concat(new string[]
			{
				@"[\x",
				((int)delim).ToString("x2"),
				@"]"
			});
		}
	}
}