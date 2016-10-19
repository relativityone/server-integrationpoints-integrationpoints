using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NSubstitute;
using kCura.IntegrationPoints.ImportProvider.Parser;
using kCura.IntegrationPoints.ImportProvider.Parser.Services;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Tests
{
    [TestFixture]
    public class EnumerableParserTests
    {
        public static IEnumerable<string> GetBasicCsvTestData()
        {
            List<string> rv = new List<string>();
            rv.Add("field-0,field-1,field-2,field-3");
            rv.Add("r1-f0,r1-f1,r1-f2,r1-f3");
            rv.Add("r2-f0,r2-f1,r2-f2,r2-f3");
            rv.Add("r3-f0,r3-f1,r3-f2,r3-f3");
            return rv;
        }

        [Test]
        public void ParsesBasicCsv()
        {
            IEnumerable<string> td = GetBasicCsvTestData();
            IEnumerator<string> tdEnum = td.GetEnumerator();
            tdEnum.MoveNext();

            EnumerableParser ep = new EnumerableParser(GetBasicCsvTestData(), ',');

            foreach (string[] current in ep)
            {
                string currentTestData = tdEnum.Current;
                tdEnum.MoveNext();

                string[] splitted = currentTestData.Split(',');
                Assert.AreEqual(current.Length, 4);
                var tdIdx = 0;
                foreach(var epColumn in current)
                {
                    Assert.AreEqual(epColumn, splitted[tdIdx++]);
                }
            }
        }
    }
}

