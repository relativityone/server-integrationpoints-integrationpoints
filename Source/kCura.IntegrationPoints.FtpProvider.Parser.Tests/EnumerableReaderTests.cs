using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FtpProvider.Parser.Tests
{
    [TestFixture, Category("Unit")]
    public class EnumerableReaderTests : TestBase
    {
        [SetUp]
        public override void SetUp()
        {
        }

        private static IEnumerable<string> GetSampleData()
        {
            var list = new List<string>()
            {
                "1abcd",
                "2abcd",
                "3abcd"
            };
            return list;
        }

        [Test]
        public void ReadLine()
        {
            var data = GetSampleData();
            var expectedData = data.Select(x => x + Environment.NewLine);

            var newlist = new List<string>();

            using (var reader = new EnumerableReader(data))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    newlist.Add(line);
                }
            }

            Assert.AreEqual(expectedData, newlist);
        }

        [Test]
        public void Read()
        {
            var data = GetSampleData();
            var expectedData = data.Select(x => x + Environment.NewLine);

            var newlist = "";

            using (var reader = new EnumerableReader(data))
            {
                int line;
                while ((line = reader.Read()) != -1)
                {
                    newlist += (char)line;
                }
            }

            Assert.AreEqual(string.Join("", expectedData), newlist);
        }

        [Test]
        public void PeekAndRead()
        {
            var data = GetSampleData();

            using (var reader = new EnumerableReader(data))
            {
                char a1h = (char)reader.Read();
                char a = (char)reader.Read();
                char bpeek = (char)reader.Peek();
                char b = (char)reader.Read();
                char c = (char)reader.Read();
                char d = (char)reader.Read();
                char r1peek = (char)reader.Peek();
                char r1 = (char)reader.Read();
                char n1 = (char)reader.Read();
                char a2hpeek = (char)reader.Peek();
                char a2hpeek2 = (char)reader.Peek();
                char a2h = (char)reader.Read();
                char a2 = (char)reader.Read();
                char b2peek = (char)reader.Peek();
                char b2peek2 = (char)reader.Peek();
                char b2 = (char)reader.Read();
                char c2 = (char)reader.Read();
                char d2 = (char)reader.Read();
                char r2peek = (char)reader.Peek();
                char r2 = (char)reader.Read();
                char n2 = (char)reader.Read();
                char a3h = (char)reader.Read();
                char a3 = (char)reader.Read();
                char b3 = (char)reader.Read();
                char c3 = (char)reader.Read();
                char d3 = (char)reader.Read();
                char r3peek = (char)reader.Peek();
                char r3 = (char)reader.Read();
                char n3 = (char)reader.Read();
                int end = reader.Read();

                Assert.AreEqual('1', a1h);
                Assert.AreEqual('a', a);
                Assert.AreEqual('b', b);
                Assert.AreEqual('b', bpeek);
                Assert.AreEqual('c', c);
                Assert.AreEqual('d', d);
                Assert.AreEqual('\r', r1peek);
                Assert.AreEqual('\r', r1);
                Assert.AreEqual('\n', n1);
                Assert.AreEqual('2', a2hpeek);
                Assert.AreEqual('2', a2hpeek2);
                Assert.AreEqual('2', a2h);
                Assert.AreEqual('a', a2);
                Assert.AreEqual('\r', r2peek);
                Assert.AreEqual('\r', r2);
                Assert.AreEqual('\n', n2);
                Assert.AreEqual('b', b2peek);
                Assert.AreEqual('b', b2peek2);
                Assert.AreEqual('b', b2);
                Assert.AreEqual('c', c2);
                Assert.AreEqual('d', d2);
                Assert.AreEqual('3', a3h);
                Assert.AreEqual('a', a3);
                Assert.AreEqual('b', b3);
                Assert.AreEqual('c', c3);
                Assert.AreEqual('d', d3);
                Assert.AreEqual('\r', r3peek);
                Assert.AreEqual('\r', r3);
                Assert.AreEqual('\n', n3);
                Assert.AreEqual(-1, end);
            }
        }

        [Test]
        public void ReadWithBuffer()
        {
            var data = GetSampleData();

            using (var reader = new EnumerableReader(data))
            {
                char[] buffer = new char[10];
                var changes = reader.Read(buffer, 0, 10);

                Assert.AreEqual(10, changes);
                Assert.AreEqual('1', buffer[0]);
                Assert.AreEqual('b', buffer[9]);
            }
        }

        [Test]
        public void ReadWithBufferWhereCountIsGreaterThanChars()
        {
            var data = GetSampleData();

            using (var reader = new EnumerableReader(data))
            {
                char[] buffer = new char[30];
                var changes = reader.Read(buffer, 0, 30);

                Assert.AreEqual(21, changes);
                Assert.AreEqual('1', buffer[0]);
                Assert.AreEqual('c', buffer[10]);
                Assert.AreEqual('3', buffer[14]);
                Assert.AreEqual('\n', buffer[20]);
                Assert.AreEqual('\0', buffer[21]);
            }
        }

        [Test]
        public void ReadToEnd()
        {
            var data = GetSampleData();

            using (var reader = new EnumerableReader(data))
            {
                var joined = reader.ReadToEnd();
                Assert.AreEqual("1abcd\r\n2abcd\r\n3abcd\r\n", joined);
            }
        }

        [Test]
        public void ReaderThrowsWhenClosed()
        {
            var data = GetSampleData();

            var reader = new EnumerableReader(data);
            reader.Dispose();

            Assert.Throws(typeof(ObjectDisposedException), () =>
           {
               reader.ReadLine();
           });
        }
    }
}
