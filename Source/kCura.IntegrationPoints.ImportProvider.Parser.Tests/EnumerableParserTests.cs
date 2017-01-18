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

		private static int TEST_ROWS = 5;
		private static int TEST_COLS = 5;

		private static IEnumerable<string> BasicTestDataHelper(char delimiter, char quoteDelimiter, int rows, int columns)
		{
			List<string> rv = new List<string>();
			rv.Add(string.Join(delimiter.ToString(), Enumerable.Range(0, columns).Select((i) =>
				quoteDelimiter + ((i == columns - 1) ? string.Empty : string.Format("field-{0}", i)) + quoteDelimiter
				)));
			foreach (int row in Enumerable.Range(0, rows))
			{
				rv.Add(string.Join(delimiter.ToString(), Enumerable.Range(0, columns).Select((i) =>
				quoteDelimiter + ((i == columns - 1) ? string.Empty : string.Format("r{0}v{1}", row, i)) + quoteDelimiter
				)));
			}
			return rv;
		}

		private void BasicParsingHelper(char recordDelimiter, char quoteDelimiter)
		{
			string[] separator = new string[] { recordDelimiter.ToString() };
			IEnumerable<string> testData = BasicTestDataHelper(recordDelimiter, quoteDelimiter, TEST_ROWS, TEST_COLS);

			EnumerableParser ep = new EnumerableParser(testData, recordDelimiter, quoteDelimiter);

			IEnumerator<string> testDataEnum = testData.GetEnumerator();
			testDataEnum.MoveNext();

			foreach (string[] currentEpOutput in ep)
			{
				string currentTestData = testDataEnum.Current;
				testDataEnum.MoveNext();

				string[] tdCols = currentTestData.Split(separator, StringSplitOptions.None).Select(x => x.Trim(quoteDelimiter)).ToArray();
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
			} while (recordDelimiter == quoteDelimiter);
			BasicParsingHelper(recordDelimiter, quoteDelimiter);
		}
	}
}

