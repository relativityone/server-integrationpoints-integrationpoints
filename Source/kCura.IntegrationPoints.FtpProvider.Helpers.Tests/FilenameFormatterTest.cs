using System;
using kCura.IntegrationPoint.Tests.Core;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FtpProvider.Helpers.Tests
{
    [TestFixture, Category("Unit")]
    public class FilenameFormatterTest : TestBase
    {
        [SetUp]
        public override void SetUp()
        {
        }

        [Test, System.ComponentModel.Description("Golden Flow")]
        [TestCase("*yyyy*-*MM*-*dd*_Custodians_BatchYear_*yyyy*_completed_*MMMM*", '*', "03/31/2016", "2016-03-31_Custodians_BatchYear_2016_completed_March")]
        [TestCase("end_*MMMM*", '*', "03/31/2016", "end_March")]
        [TestCase("*yyyy*_end_*MMMM*", '*', "03/31/2016", "2016_end_March")]
        [TestCase("NoReplacementsTest", '*', "03/31/2016", "NoReplacementsTest")]
        [TestCase("", '*', "03/31/2016", "")]
        [TestCase("*yyyy**MM**dd*", '*', "03/31/2016", "20160331")]
        [TestCase(" *yyyy**MM**dd*", '*', "03/31/2016", " 20160331")]
        [TestCase("*yyyy**MM**dd* ", '*', "03/31/2016", "20160331 ")]
        [TestCase(" *yyyy**MM**dd* ", '*', "03/31/2016", " 20160331 ")]
        [TestCase(" NoReplacementsSpacesOnEnd ", '*', "03/31/2016", " NoReplacementsSpacesOnEnd ")]
        [TestCase(" NoReplacementsSpaceOnLeftEnd", '*', "03/31/2016", " NoReplacementsSpaceOnLeftEnd")]
        [TestCase("NoReplacementsSpaceOnRightEnd ", '*', "03/31/2016", "NoReplacementsSpaceOnRightEnd ")]
        [TestCase("*yyyy*-*MM*-*dd*-*HH*_Test.csv", '*', "04/05/2016 19:14", "2016-04-05-19_Test.csv")]

        public void GoldenFlow(string filename, char wildCard, string date, string expected)
        {
            var dateObj = Convert.ToDateTime(date);
            var result = FilenameFormatter.FormatFilename(filename, wildCard, dateObj);
            Assert.IsTrue(String.Compare(result, expected, StringComparison.Ordinal) == 0);
        }

        [Test, System.ComponentModel.Description("Finds all indexes of wildcards")]
        public void ValidateThatAllIndexesOfWildCardsAreFound()
        {
            var filename = "*Hello*My*NameIs**Marlon*";
            var result = FilenameFormatter.FindAllIndexes(filename, '*');
            int[] expected = { 0, 6, 9, 16, 17, 24 };

            Assert.AreEqual(result.Length, expected.Length);
            for (var i = 0; i < expected.Length; i++)
            {
                Assert.IsTrue(expected[i] == result[i]);
            }
        }
    }
}
