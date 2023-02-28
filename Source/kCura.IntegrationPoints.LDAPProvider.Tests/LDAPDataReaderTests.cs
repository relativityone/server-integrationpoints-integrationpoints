using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Relativity.API;

namespace kCura.IntegrationPoints.LDAPProvider.Tests
{
    [TestFixture, Category("Unit")]
    public class LDAPDataReaderTests
    {
        private List<string> _headers;
        private LDAPSettings _ldapSettings;
        private LDAPDataReader _reader;

        [SetUp]
        public void CreateProvider()
        {
            _headers = new List<string>
            {
                "Name",
                "Age",
                "Favourite float",
                "Born",
                "Gift from Saint Nicholas",
                "Sex",
                "Single",
                "Some float",
                "Some byte",
                "Some decimal",
                "Some GUID",
                "Some Int16",
                "Some Int64"
            };
            _ldapSettings = new LDAPSettings();
            var helper = NSubstitute.Substitute.For<IHelper>();
            _reader = new LDAPDataReader(
                CreateSearchResults(_headers, new[]
                {
                    new object[]
                    {
                        new[] {"Bob", "Zdzislaw"}, 22, 3.1415, new DateTime(1900, 1, 11, 1, 1, 1), DBNull.Value, "M",
                        true, 3.2f, 255, 45.5222m, new Guid("12345678-9abc-def0-1234-012345678901"), 32767, 41234567890
                    },
                    new object[]
                    {
                        "Alice", 33, 2.7182, new DateTime(1922, 1, 22, 1, 2, 3), "Shuttle", "Y", null, 3.6f, 0, 0m,
                        new Guid("12345678-9abc-def0-1234-012345678901"), 32767, 41234567890
                    }
                }),
                _headers,
                new LDAPDataFormatterDefault(_ldapSettings, helper)
            );
        }

        [Test]
        public void HaveNotHaveBatchResults()
        {
            // Act / Assert
            _reader.NextResult().Should().BeFalse();
            _reader.Depth.Should().Be(0);
        }

        [Test]
        public void ReturnProperValuesByGetters()
        {
            // Act / Assert
            _reader.Read().Should().BeTrue();

            object val = _reader.GetValue(0);
            (val is string).Should().BeTrue();
            val.Should().Be("Bob" + _ldapSettings.MultiValueDelimiter + "Zdzislaw");

            _reader.GetString(0).Should().Be("Bob" + _ldapSettings.MultiValueDelimiter + "Zdzislaw");

            _reader.GetValue(1).Should().Be("22");

            _reader.GetInt32(1).Should().Be(22);

            _reader.GetValue(2).Should().Be("3.1415");
            _reader.GetDouble(2).Should().Be(3.1415);

            _reader.GetValue(3).Should().Be(new DateTime(1900, 1, 11, 1, 1, 1).ToString("s"));
            _reader.GetDateTime(3).Should().Be(new DateTime(1900, 1, 11, 1, 1, 1));

            // TODO faulty, but it's due to formatter used in Reader returning "" for null
            _reader.GetValue(4).Should().Be("");

            _reader.GetChar(5).Should().Be('M');
            _reader.IsDBNull(5).Should().BeFalse();

            _reader.GetBoolean(6).Should().BeTrue();

            _reader.GetFloat(7).Should().Be(3.2f);

            _reader.GetByte(8).Should().Be(255);

            _reader.GetDataTypeName(0).Should().Be("String");
            _reader.GetDataTypeName(8).Should().Be("String");
            _reader.GetFieldType(10).Should().Be(typeof(string));

            _reader.GetDecimal(9).Should().Be(45.5222m);

            _reader.GetGuid(10).Should().Be(new Guid("12345678-9abc-def0-1234-012345678901"));

            _reader.GetInt16(11).Should().Be(32767);

            _reader.GetInt64(12).Should().Be(41234567890);
        }

        [Test]
        public void ChangeRecordsAffectedOnRecordChange()
        {
            // Act / Assert
            _reader.RecordsAffected.Should().Be(0);

            _reader.Read().Should().BeTrue();
            _reader.RecordsAffected.Should().Be(1);

            _reader.Read().Should().BeTrue();
            _reader.RecordsAffected.Should().Be(2);
        }

        [Test]
        public void DontChangeRecordsAffectedAfterAllDataRead()
        {
            // Act / Assert
            _reader.Read().Should().BeTrue();
            _reader.Read().Should().BeTrue();
            _reader.RecordsAffected.Should().Be(2);

            _reader.Read().Should().BeFalse();
            _reader.RecordsAffected.Should().Be(2);
        }

        [Test]
        public void CloseReaderAfterAllDataRead()
        {
            // Act / Assert
            _reader.IsClosed.Should().BeFalse();

            bool isOpen;
            do
            {
                isOpen = _reader.Read();
            } while (isOpen);

            _reader.IsClosed.Should().BeTrue();
        }

        [Test]
        public void ThrowExceptionOnInvokingUnimplementedGetBytesMethod()
        {
            // Act / Assert
            Assert.Throws<NotImplementedException>(() => _reader.GetBytes(0, 0, null, 0, 0));
        }

        [Test]
        public void ThrowExceptionOnInvokingUnimplementedGetCharsMethod()
        {
            // Act / Assert
            Assert.Throws<NotImplementedException>(() => _reader.GetChars(0, 0, null, 0, 0));
        }

        [Test]
        public void ThrowExceptionOnInvokingUnimplementedGetDataMethod()
        {
            // Act / Assert
            Assert.Throws<NotImplementedException>(() => _reader.GetData(0));
        }

        [Test]
        public void ThrowExceptionOnAccessingNonexistentColumn()
        {
            // Act / Assert
            Action act = () => _reader.GetValue(13);
            act.ShouldThrow<NullReferenceException>();
        }

        [Test]
        public void PopulateColumnsInSchemaTable()
        {
            for (var i = 0; i < _headers.Count; ++i)
            {
                _reader.GetSchemaTable().Columns[i].ColumnName.Should().Be(_headers[i]);
            }
        }

        [Test]
        public void ReturnRangeOfValuesOfTheSameLengthAsColumns()
        {
            // Arrange
            _reader.Read();
            var values = new object[_headers.Count];

            // Act
            _reader.GetValues(values).Should().Be(_headers.Count);

            // Assert
            foreach (string header in _headers)
            {
                values[_reader.GetOrdinal(header)].Should().Be(_reader[header]);
            }
        }

        [Test]
        public void ReturnRangeOfValuesShorterThanColumnsCount()
        {
            // Arrange
            _reader.Read();
            int valuesCount = _headers.Count - 1;
            var values = new object[valuesCount];

            // Act
            _reader.GetValues(values).Should().Be(valuesCount);

            // Assert
            for (var i = 0; i < valuesCount; ++i)
            {
                values[i].Should().Be(_reader.GetValue(i));
            }
        }

        [Test]
        public void ReturnRangeOfValuesLongerThanColumnsCount()
        {
            // Arrange
            _reader.Read();
            int valuesCount = _headers.Count + 1;
            var values = new object[valuesCount];

            // Act
            _reader.GetValues(values).Should().Be(_headers.Count);

            // Assert
            for (var i = 0; i < _headers.Count; ++i)
            {
                values[i].Should().Be(_reader.GetValue(i));
            }
            for (int i = _headers.Count; i < valuesCount; ++i)
            {
                values[i].Should().BeNull();
            }
        }

        [Test]
        public void DoNotReturnAnythingIfArrayProvidedToGetValuesIsNull()
        {
            // Arrange
            _reader.Read();

            // Act / Assert
            _reader.GetValues(null).Should().Be(0);
        }

        [Test]
        public void CloseReaderOnCloseInvocation()
        {
            // Act
            _reader.Close();

            // Assert
            _reader.IsClosed.Should().BeTrue();
            _reader.Read().Should().BeFalse();
        }

        [Test]
        public void CloseReaderOnDisposeInvocation()
        {
            // Act
            _reader.Dispose();

            // Assert
            _reader.IsClosed.Should().BeTrue();
            _reader.Read().Should().BeFalse();
        }

        /// <summary>Creates collection of SearchResults used in <see cref="LDAPDataReader"/> constructor</summary>
        /// <remarks>
        /// Each SearchResult creates empty collection of ResultPropertyCollection at creation time. <br/>
        /// ResultPropertyCollection is a map containing ResultPropertyValueCollection as values identified by string keys.<br/>
        /// Keys corresponds to property names in LDAP.<br/>
        /// Values corresponds to values of properties in LDAP.<br/>
        /// As collection is used as a value, each property can have many values.<br/>
        /// </remarks>
        /// <returns>collection of SearchResults.</returns>
        /// <param name="names">names of LDAP attributes (columns in <see cref="LDAPDataReader._schemaTable"/>).</param>
        /// <param name="values">values for parametes. If given value is an array then multi-value property is created.</param>
        private static IEnumerable<SearchResult> CreateSearchResults(IReadOnlyList<string> names,
            IEnumerable<object[]> values)
        {
            var searchResults = new List<SearchResult>();
            foreach (object[] value in values)
            {
                ConstructorInfo searchResultCtor = typeof(SearchResult)
                    .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                    .First();
                var searchResult = (SearchResult) searchResultCtor.Invoke(new object[] {null, null});

                for (var i = 0; i < names.Count; ++i)
                {
                    ConstructorInfo valueCollectionCtor = typeof(ResultPropertyValueCollection)
                        .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                        .First();
                    ResultPropertyValueCollection valueCollection = value[i] is Array
                        ? (ResultPropertyValueCollection) valueCollectionCtor.Invoke(new[] {value[i]})
                        : (ResultPropertyValueCollection) valueCollectionCtor.Invoke(new object[] {new[] {value[i]}});

                    MethodInfo addMethod = typeof(ResultPropertyCollection)
                        .GetMethod("Add", BindingFlags.Instance | BindingFlags.NonPublic);
                    addMethod.Invoke(searchResult.Properties, new object[] {names[i], valueCollection});
                }

                searchResults.Add(searchResult);
            }
            return searchResults;
        }
    }
}
