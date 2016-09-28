﻿using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Services.Keywords;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Keywords
{
	[TestFixture]
	public class KeywordConverterTests
	{
		[Test]
		public void Convert_KeywordDifferentCaseReturnAnOutput_CorrectOutput()
		{
			//ARRANGE
			string keyword = "\\[Bla]";
			string input = string.Format("Hello, [bLa]{0}World!!!", Environment.NewLine);
			IEnumerable<IKeyword> keywords = new List<IKeyword>()
	  {
		new mockKeyword(keyword, Environment.NewLine + "Great")
	  };
			KeywordFactory keywordFactory = new KeywordFactory(keywords);
			KeywordConverter keywordConverter = new KeywordConverter(keywordFactory);


			//ACT
			string output = keywordConverter.Convert(input);


			//ASSERT
			Assert.AreEqual(@"Hello, 
Great
World!!!", output);
		}

		[Test]
		public void Convert_KeywordIsNotTheOnlyEntryOnTheLine_CorrectOutput()
		{
			//ARRANGE
			string keyword = "\\[Bla]";
			string input = string.Format("Hello, [bLa]{0}World!!!", Environment.NewLine);
			IEnumerable<IKeyword> keywords = new List<IKeyword>()
	  {
		new mockKeyword(keyword, string.Empty)
	  };
			KeywordFactory keywordFactory = new KeywordFactory(keywords);
			KeywordConverter keywordConverter = new KeywordConverter(keywordFactory);


			//ACT
			string output = keywordConverter.Convert(input);


			//ASSERT
			Assert.AreEqual(@"Hello, 
World!!!", output);
		}

		[Test]
		public void Convert_KeywordIsTheOnlyEntryOnTheLine_CorrectOutput()
		{
			//ARRANGE
			string keyword = "\\[Bla]";
			string input = string.Format("Hello, {0}[bLa]{0}World!!!", Environment.NewLine);

			IEnumerable<IKeyword> keywords = new List<IKeyword>()
	  {
		new mockKeyword(keyword, string.Empty)
	  };

			KeywordFactory keywordFactory = new KeywordFactory(keywords);
			KeywordConverter keywordConverter = new KeywordConverter(keywordFactory);


			//ACT
			string output = keywordConverter.Convert(input);


			//ASSERT
			Assert.AreEqual(@"Hello, World!!!", output);
		}

		[Test]
		public void Convert_KeywordIsBothTheOnlyEntryOnTheLineAndNot_CorrectOutput()
		{
			//ARRANGE
			string keyword = "\\[Bla]";
			string input = string.Format("Hello, {0}[bLa]{0}{0}this [bLa] World!!!", Environment.NewLine);

			IEnumerable<IKeyword> keywords = new List<IKeyword>()
	  {
		new mockKeyword(keyword, string.Empty)
	  };

			KeywordFactory keywordFactory = new KeywordFactory(keywords);
			KeywordConverter keywordConverter = new KeywordConverter(keywordFactory);


			//ACT
			string output = keywordConverter.Convert(input);


			//ASSERT
			Assert.AreEqual(@"Hello, 
this  World!!!", output);
		}
	}

	internal class mockKeyword : IKeyword
	{
		private string _newValue;
		public mockKeyword(string keywordName, string newValue)
		{
			KeywordName = keywordName;
			_newValue = newValue;
		}
		public string KeywordName { get; }
		public string Convert()
		{
			return _newValue;
		}
	}
}
