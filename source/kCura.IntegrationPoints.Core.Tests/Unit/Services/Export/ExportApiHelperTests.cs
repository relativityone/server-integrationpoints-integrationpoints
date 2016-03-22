using System;
using kCura.IntegrationPoints.Core.Services.Exporter;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Unit.Services.Export
{
	[TestFixture]
	public class ExportApiHelperTests
	{
		private const String MultiObjectParsingError = "Encountered an error while processing multi-object field, this may due to out-of-date version of the software. Please contact administrator for more information.";

		[TestCase("<object>Abc</object>", Result = "Abc")]
		[TestCase("<object>abc</object><object>def</object>", Result = "abc;def")]
		[TestCase(null, Result = null)]
		[TestCase("", Result = "")]
		[TestCase("NonXmlString", ExpectedException = typeof(Exception), ExpectedMessage = MultiObjectParsingError)]
		[TestCase("<object><object></object>", ExpectedException = typeof(Exception), ExpectedMessage = MultiObjectParsingError)]
		[TestCase("<object></object></object>", ExpectedException = typeof(Exception), ExpectedMessage = MultiObjectParsingError)]
		[TestCase("<object>abc</object><object>def</object><object>abc</object>", Result = "abc;def;abc")]
		public object MultiObjectFieldParsing(object input)
		{
			return ExportApiDataHelper.SanitizeMultiObjectField(input);
		}

		[TestCase("\vChoice1", Result = "Choice1")]
		[TestCase("\vChoice1&#x0B;", Result = "Choice1")]
		[TestCase("\vChoice1&#x0B;&#x0B;&#x0B;&#x0B;", Result = "Choice1")]
		[TestCase("Choice1&#x0B;", Result = "Choice1")]
		[TestCase(null, Result = null)]
		[TestCase("", Result = "")]
		public object SingleChoiceFieldParsing(object input)
		{
			return ExportApiDataHelper.SanitizeSingleChoiceField(input);
		}
	}
}