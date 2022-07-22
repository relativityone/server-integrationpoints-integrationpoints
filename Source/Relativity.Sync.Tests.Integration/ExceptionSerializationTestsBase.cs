using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Relativity.Sync.Tests.Integration
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    internal abstract class ExceptionSerializationTestsBase<T> where T : Exception, new()
    {
        [Test]
        public void ItShouldSerializeToXml()
        {
            const int bufferSize = 4096;

            ArgumentException innerEx = new ArgumentException("foo");
            T originalException = CreateException("message", innerEx);
            byte[] buffer = new byte[bufferSize];
            MemoryStream ms = new MemoryStream(buffer);
            MemoryStream ms2 = new MemoryStream(buffer);
            BinaryFormatter formatter = new BinaryFormatter();

            // ACT
            formatter.Serialize(ms, originalException);
            T deserializedException = (T) formatter.Deserialize(ms2);

            // ASSERT
            deserializedException.InnerException.Should().NotBeNull();
            deserializedException.InnerException.Message.Should().Be(originalException.InnerException.Message);
            deserializedException.Message.Should().Be(originalException.Message);
        }

        [Test]
        public void ItShouldSerializeToJson()
        {
            ArgumentException innerEx = new ArgumentException("foo");
            T originalException = CreateException("message", innerEx);

            // ACT
            string json = JsonConvert.SerializeObject(originalException);
            T deserializedException = JsonConvert.DeserializeObject<T>(json);

            // ASSERT
            deserializedException.InnerException.Should().NotBeNull();
            deserializedException.InnerException.Message.Should().Be(originalException.InnerException.Message);
            deserializedException.Message.Should().Be(originalException.Message);
        }

        protected virtual T CreateException(string message, Exception innerException)
        {
            return CreateUsingInnerExceptionConstructor(message, innerException);
        }

        private static T CreateUsingInnerExceptionConstructor(string message, Exception innerException)
        {
            object[] constructorArgs = { message, innerException };
            ConstructorInfo constructor = FindInnerExceptionConstructor(typeof(T));
            return (T) constructor.Invoke(constructorArgs);
        }

        private static ConstructorInfo FindInnerExceptionConstructor(Type exceptionType)
        {
            ConstructorInfo properConstructor = exceptionType.GetConstructors().FirstOrDefault(x =>
            {
                ParameterInfo[] parameters = x.GetParameters();
                const int parameterLength = 2;
                return
                    parameters.Length == parameterLength &&
                    parameters[0].ParameterType == typeof(string) &&
                    parameters[1].ParameterType == typeof(Exception);
            });

            if (properConstructor == null)
            {
                throw new ArgumentException($"Exception type {exceptionType.FullName} does not contain a two-argument constructor that accepts a {typeof(string)} and {typeof(Exception)}.");
            }

            return properConstructor;
        }
    }
}
