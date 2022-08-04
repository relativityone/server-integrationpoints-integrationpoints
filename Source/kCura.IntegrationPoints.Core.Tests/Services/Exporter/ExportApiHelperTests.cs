using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services.Exporter;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Services.Exporter
{
    [TestFixture, Category("Unit")]
    public class ExportApiHelperTests : TestBase
    {
        [SetUp]
        public override void SetUp() { }

        private const String MultiObjectParsingError = "Encountered an error while processing multi-object field, this may due to out-of-date version of the software. Please contact administrator for more information.";

        [TestCase("<object>Abc</object>", "Abc")]
        [TestCase("<object>abc</object><object>def</object>", "abc;def")]
        [TestCase(null, null)]
        [TestCase("", "")]
        [TestCase("<object>abc</object><object>def</object><object>abc</object>", "abc;def;abc")]
        public void MultiObjectFieldParsing(object input, object expectedResult)
        {
            object result = ExportApiDataHelper.SanitizeMultiObjectField(input);
            Assert.AreEqual(expectedResult, result);
        }

        [TestCase("NonXmlString", MultiObjectParsingError)]
        [TestCase("<object><object></object>", MultiObjectParsingError)]
        [TestCase("<object></object></object>", MultiObjectParsingError)]
        public void MultiObjectFieldParsing_Exceptions(object input, string expectedExceptionMessage)
        {
            Assert.That(() => ExportApiDataHelper.SanitizeMultiObjectField(input),
                Throws.Exception
                    .TypeOf<Exception>()
                    .With.Property("Message")
                    .EqualTo(expectedExceptionMessage));
        }

        [TestCase("\vChoice1", "Choice1")]
        [TestCase("\vChoice1&#x0B;", "Choice1")]
        [TestCase("\vChoice1&#x0B;&#x0B;&#x0B;&#x0B;", "Choice1")]
        [TestCase("Choice1&#x0B;", "Choice1")]
        [TestCase(null, null)]
        [TestCase("", "")]
        public void SingleChoiceFieldParsing(object input, object expectedResult)
        {
            object result = ExportApiDataHelper.SanitizeSingleChoiceField(input);
            Assert.AreEqual(expectedResult, result);
        }

    }
}