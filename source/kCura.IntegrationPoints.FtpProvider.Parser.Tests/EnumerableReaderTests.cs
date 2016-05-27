using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FtpProvider.Parser.Tests
{
    public class EnumerableReaderTests
    {
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

            var newlist = new List<string>();

            using (var reader = new EnumerableReader(data))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    newlist.Add(line);
                }
            }

            Assert.AreEqual(data, newlist);
        }

        [Test]
        public void Read()
        {
            var data = GetSampleData();

            var newlist = "";

            using (var reader = new EnumerableReader(data))
            {
                int line;
                while ((line = reader.Read()) != -1)
                {
                    newlist += (char)line;
                }
            }

            Assert.AreEqual(string.Join("", data), newlist);
        }

        [Test]
        public void ReadWithSeparator()
        {
            var data = GetSampleData();

            var newlist = "";

            using (var reader = new EnumerableReader(data, '\n'))
            {
                int line;
                while ((line = reader.Read()) != -1)
                {
                    newlist += (char)line;
                }
            }

            Assert.AreEqual(string.Join("\n", data), newlist);
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
                char a2hpeek = (char)reader.Peek();
                char a2hpeek2 = (char)reader.Peek();
                char a2h = (char)reader.Read();
                char a2 = (char)reader.Read();
                char b2peek = (char)reader.Peek();
                char b2peek2 = (char)reader.Peek();
                char b2 = (char)reader.Read();
                char c2 = (char)reader.Read();
                char d2 = (char)reader.Read();
                char a3h = (char)reader.Read();
                char a3 = (char)reader.Read();
                char b3 = (char)reader.Read();
                char c3 = (char)reader.Read();
                char d3 = (char)reader.Read();
                int end = reader.Read();

                Assert.AreEqual('1', a1h);
                Assert.AreEqual('a', a);
                Assert.AreEqual('b', b);
                Assert.AreEqual('b', bpeek);
                Assert.AreEqual('c', c);
                Assert.AreEqual('d', d);
                Assert.AreEqual('2', a2hpeek);
                Assert.AreEqual('2', a2hpeek2);
                Assert.AreEqual('2', a2h);
                Assert.AreEqual('a', a2);
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
                Assert.AreEqual(-1, end);
            }
        }

        [Test]
        public void PeekAndReadWithSeparator()
        {
            var data = GetSampleData();

            using (var reader = new EnumerableReader(data, '\n'))
            {
                char a1h = (char)reader.Read();
                char a = (char)reader.Read();
                char bpeek = (char)reader.Peek();
                char b = (char)reader.Read();
                char c = (char)reader.Read();
                char d = (char)reader.Read();
                char n1peek = (char)reader.Peek();
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
                char n2 = (char)reader.Read();
                char a3h = (char)reader.Read();
                char a3 = (char)reader.Read();
                char b3 = (char)reader.Read();
                char c3 = (char)reader.Read();
                char d3 = (char)reader.Read();
                int end = reader.Read();

                Assert.AreEqual('1', a1h);
                Assert.AreEqual('a', a);
                Assert.AreEqual('b', b);
                Assert.AreEqual('b', bpeek);
                Assert.AreEqual('c', c);
                Assert.AreEqual('d', d);
                Assert.AreEqual('\n', n1peek);
                Assert.AreEqual('\n', n1);
                Assert.AreEqual('2', a2hpeek);
                Assert.AreEqual('2', a2hpeek2);
                Assert.AreEqual('2', a2h);
                Assert.AreEqual('a', a2);
                Assert.AreEqual('b', b2peek);
                Assert.AreEqual('b', b2peek2);
                Assert.AreEqual('b', b2);
                Assert.AreEqual('c', c2);
                Assert.AreEqual('d', d2);
                Assert.AreEqual('\n', n2);
                Assert.AreEqual('3', a3h);
                Assert.AreEqual('a', a3);
                Assert.AreEqual('b', b3);
                Assert.AreEqual('c', c3);
                Assert.AreEqual('d', d3);
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
                Assert.AreEqual('d', buffer[9]);
            }
        }

        [Test]
        public void ReadWithBufferWhereCountIsGreaterThanChars()
        {
            var data = GetSampleData();

            using (var reader = new EnumerableReader(data))
            {
                char[] buffer = new char[20];
                var changes = reader.Read(buffer, 0, 20);

                Assert.AreEqual(15, changes);
                Assert.AreEqual('1', buffer[0]);
                Assert.AreEqual('3', buffer[10]);
                Assert.AreEqual('d', buffer[14]);
            }
        }

        [Test]
        public void ReadToEnd()
        {
            var data = GetSampleData();

            using (var reader = new EnumerableReader(data))
            {
                var joined = reader.ReadToEnd();
                Assert.AreEqual("1abcd2abcd3abcd", joined);
            }
        }

        [Test]
        public void ReadToEndWithSeparator()
        {
            var data = GetSampleData();

            using (var reader = new EnumerableReader(data, '\n'))
            {
                var joined = reader.ReadToEnd();
                Assert.AreEqual("1abcd\n2abcd\n3abcd", joined);
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