using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using NUnit.Framework;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Tests
{
	[TestFixture]
	public class EnumerableParserTests : TestBase
	{
		[SetUp]
		public override void SetUp()
		{

		}

		private static int MAX_ROWS = 5;

		private static int MAX_COLS = 5;

		private static IEnumerable<string> BasicTestDataHelper(char delimiter, int rows, int columns)
		{
			List<string> rv = new List<string>();
			rv.Add(string.Join(delimiter.ToString(), Enumerable.Range(0, columns).Select((i) => string.Format("field-{0}", i))));
			foreach (int row in Enumerable.Range(0, rows))
			{
				rv.Add(string.Join(delimiter.ToString(), Enumerable.Range(0, columns).Select((i) => string.Format("r{0}v{1}", row, i))));
			}
			return rv;
		}

		private void BasicParsingHelper(char recordDelimiter, char quoteDelimiter)
		{
			string[] separator = new string[] { recordDelimiter.ToString() };
			Random randGen = new Random();
			int rows = randGen.Next(MAX_ROWS);
			int cols = randGen.Next(MAX_COLS);
			IEnumerable<string> testData = BasicTestDataHelper(recordDelimiter, rows, cols);

			EnumerableParser ep = new EnumerableParser(testData, recordDelimiter, quoteDelimiter);

			IEnumerator<string> testDataEnum = testData.GetEnumerator();
			testDataEnum.MoveNext();

			foreach (string[] currentEpOutput in ep)
			{
				string currentTestData = testDataEnum.Current;
				testDataEnum.MoveNext();

				string[] tdCols = currentTestData.Split(separator, StringSplitOptions.RemoveEmptyEntries);
				Assert.AreEqual(currentEpOutput.Length, tdCols.Length);
				int tdIdx = 0;
				foreach (string epColumn in currentEpOutput)
				{
					Assert.AreEqual(epColumn, tdCols[tdIdx++]);
				}
			}
		}

		[Test]
		public void ParsesBasicCsvData()
		{
			BasicParsingHelper(',', '"');
		}

		[Test]
		public void ParsesBasicPipeData()
		{
			BasicParsingHelper('|', '"');
		}

		[Test]
		public void ParsesUnprintableDelimiterData()
		{
            char quoteDelimiter = (char)(new Random()).Next(1, 32);
            char recordDelimiter;
            do
            {
                recordDelimiter = (char)(new Random()).Next(1, 32);
            } while (recordDelimiter != quoteDelimiter);
            BasicParsingHelper(recordDelimiter, quoteDelimiter);
		}
	}
}

