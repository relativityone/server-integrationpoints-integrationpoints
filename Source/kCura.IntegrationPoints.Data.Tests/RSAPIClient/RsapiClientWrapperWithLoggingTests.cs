using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using kCura.IntegrationPoints.Data.RSAPIClient;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.Relativity.Client;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Tests.RSAPIClient
{
	[TestFixture]
	public class RsapiClientWrapperWithLoggingTests
	{
		[Test]
		public void ItShouldCallMethodsAndPropertiesAndEventsOfDecoratedObject()
		{
			IRSAPIClient decoratedObject = Substitute.For<IRSAPIClient>();
			IAPILog logger = Substitute.For<IAPILog>();
			var decorator = new RsapiClientWrapperWithLogging(decoratedObject, logger);

			Type interfaceType = typeof(IRSAPIClient);
			MethodInfo[] methodsFromInterface = GetAllMethodsAndPropertiesFromInterfaceAndDerivedInterfaces(interfaceType).ToArray();

			var methodEmptyParams = new Dictionary<MethodInfo, object[]>();

			foreach (MethodInfo methodInfo in methodsFromInterface)
			{
				object[] parameters = methodInfo
					.GetParameters()
					.Select(x => x.ParameterType)
					.Select(GetDefaultValue)
					.ToArray();

				methodEmptyParams[methodInfo] = parameters;
			}

			// Act
			foreach (MethodInfo methodInfo in methodsFromInterface)
			{
				methodInfo.Invoke(decorator, methodEmptyParams[methodInfo]);
			}

			// Assert
			foreach (MethodInfo methodInfo in methodsFromInterface)
			{
				IRSAPIClient receivedValidator = decoratedObject.ReceivedWithAnyArgs(); // new instance of validator have to be created for each method
				methodInfo.Invoke(receivedValidator, methodEmptyParams[methodInfo]);
			}
		}

		[Test]
		public void ItShouldLogAndRethrowExceptionInMethodsOfDecoratedObject()
		{
			IRSAPIClient decoratedObject = Substitute.For<IRSAPIClient>();
			IAPILog logger = Substitute.For<IAPILog>();
			logger.ForContext<RsapiClientWrapperWithLogging>().Returns(logger);
			var decorator = new RsapiClientWrapperWithLogging(decoratedObject, logger);

			Type interfaceType = typeof(IRSAPIClient);
			MethodInfo[] methodsFromInterface = GetAllMethodsFromInterfaceAndDerivedInterfaces(interfaceType).ToArray();

			var methodEmptyParams = new Dictionary<MethodInfo, object[]>();

			foreach (MethodInfo methodInfo in methodsFromInterface)
			{
				object[] parameters = methodInfo
					.GetParameters()
					.Select(x => x.ParameterType)
					.Select(GetDefaultValue)
					.ToArray();

				methodEmptyParams[methodInfo] = parameters;
			}

			foreach (MethodInfo methodInfo in methodsFromInterface)
			{
				decoratedObject.When(x => methodInfo.Invoke(x, methodEmptyParams[methodInfo])).Do(x => { throw new Exception(); });
			}

			foreach (MethodInfo methodInfo in methodsFromInterface)
			{
				try
				{
					methodInfo.Invoke(decorator, methodEmptyParams[methodInfo]);
					Assert.Fail(); // Assert exception is rethrown
				}
				catch (TargetInvocationException e) when (e.InnerException is IntegrationPointsException) { }

				logger.Received().LogError(Arg.Any<Exception>(), Arg.Any<string>(), methodInfo.Name); // Asser error is logged
			}
		}

		[Test]
		public void ItShouldLogAndRethrowExceptionInPropertiesOfDecoratedObject()
		{
			IRSAPIClient decoratedObject = Substitute.For<IRSAPIClient>();
			IAPILog logger = Substitute.For<IAPILog>();
			logger.ForContext<RsapiClientWrapperWithLogging>().Returns(logger);
			var decorator = new RsapiClientWrapperWithLogging(decoratedObject, logger);

			Type interfaceType = typeof(IRSAPIClient);
			PropertyInfo[] propertiesFromInterface = GetAllPropertiesFromInterfaceAndDerivedInterfaces(interfaceType).ToArray();

			var propertyEmptyValues = new Dictionary<PropertyInfo, object>();
			foreach (PropertyInfo methodInfo in propertiesFromInterface)
			{
				object parameters = GetDefaultValue(methodInfo.PropertyType);
				propertyEmptyValues[methodInfo] = parameters;
			}

			foreach (PropertyInfo propertyInfo in propertiesFromInterface)
			{
				if (propertyInfo.CanRead)
				{
					decoratedObject.When(x => propertyInfo.GetValue(x)).Do(x => { throw new Exception(); });
				}
				if (propertyInfo.CanWrite)
				{
					decoratedObject.When(x => propertyInfo.SetValue(x, propertyEmptyValues[propertyInfo]))
						.Do(x => { throw new Exception(); });
				}
			}

			foreach (PropertyInfo propertyInfo in propertiesFromInterface)
			{
				if (propertyInfo.CanRead)
				{
					try
					{
						propertyInfo.GetValue(decorator);
						Assert.Fail(); // Assert exception is rethrown
					}
					catch (TargetInvocationException e) when (e.InnerException is IntegrationPointsException)
					{
					}

					logger.Received().LogError(Arg.Any<Exception>(), Arg.Any<string>(), propertyInfo.Name); // Asser error is logged
				}

				if (propertyInfo.CanWrite)
				{
					try
					{
						propertyInfo.SetValue(decorator, propertyEmptyValues[propertyInfo]);
						Assert.Fail(); // Assert exception is rethrown
					}
					catch (TargetInvocationException e) when (e.InnerException is IntegrationPointsException)
					{
					}

					logger.Received().LogError(Arg.Any<Exception>(), Arg.Any<string>(), propertyInfo.Name); // Asser error is logged
				}
			}
		}

		[Test]
		public void ItShouldLogAndRethrowExceptionInEventsOfDecoratedObject()
		{
			IRSAPIClient decoratedObject = Substitute.For<IRSAPIClient>();
			IAPILog logger = Substitute.For<IAPILog>();
			logger.ForContext<RsapiClientWrapperWithLogging>().Returns(logger);
			var decorator = new RsapiClientWrapperWithLogging(decoratedObject, logger);

			Type interfaceType = typeof(IRSAPIClient);
			EventInfo[] eventsFromInterface = GetAllEventsFromInterfaceAndDerivedInterfaces(interfaceType).ToArray();

			foreach (EventInfo eventInfo in eventsFromInterface)
			{
				decoratedObject.When(x => eventInfo.AddEventHandler(x, Arg.Any<Delegate>())).Do(x => { throw new Exception(); });
				decoratedObject.When(x => eventInfo.RemoveEventHandler(x, Arg.Any<Delegate>())).Do(x => { throw new Exception(); });
			}

			foreach (EventInfo propertyInfo in eventsFromInterface)
			{
				try
				{
					propertyInfo.AddEventHandler(decorator, null);
					Assert.Fail(); // Assert exception is rethrown
				}
				catch (TargetInvocationException e) when (e.InnerException is IntegrationPointsException)
				{
				}

				logger.Received().LogError(Arg.Any<Exception>(), Arg.Any<string>(), propertyInfo.Name); // Asser error is logged


				try
				{
					propertyInfo.RemoveEventHandler(decorator, null);
					Assert.Fail(); // Assert exception is rethrown
				}
				catch (TargetInvocationException e) when (e.InnerException is IntegrationPointsException)
				{
				}

				logger.Received().LogError(Arg.Any<Exception>(), Arg.Any<string>(), propertyInfo.Name); // Asser error is logged
			}
		}

		private IEnumerable<PropertyInfo> GetAllPropertiesFromInterfaceAndDerivedInterfaces(Type interfaceType)
		{
			IEnumerable<Type> allInterfaces = GetAllInterfaces(interfaceType);

			return allInterfaces
				.Select(x => (IEnumerable<PropertyInfo>)x.GetProperties())
				.Aggregate((x, y) => x.Concat(y));
		}
		private IEnumerable<EventInfo> GetAllEventsFromInterfaceAndDerivedInterfaces(Type interfaceType)
		{
			IEnumerable<Type> allInterfaces = GetAllInterfaces(interfaceType);

			return allInterfaces
				.Select(x => (IEnumerable<EventInfo>)x.GetEvents())
				.Aggregate((x, y) => x.Concat(y));
		}

		private IEnumerable<MethodInfo> GetAllMethodsFromInterfaceAndDerivedInterfaces(Type interfaceType)
		{
			return GetAllMethodsAndPropertiesFromInterfaceAndDerivedInterfaces(interfaceType).Where(x => !x.IsSpecialName);
		}

		private IEnumerable<MethodInfo> GetAllMethodsAndPropertiesFromInterfaceAndDerivedInterfaces(Type interfaceType)
		{
			IEnumerable<Type> allInterfaces = GetAllInterfaces(interfaceType);

			return allInterfaces
				.Select(x => (IEnumerable<MethodInfo>)x.GetMethods())
				.Aggregate((x, y) => x.Concat(y));
		}

		private IEnumerable<Type> GetAllInterfaces(Type t)
		{
			IEnumerable<Type> allImplementedInterfeces = new List<Type> { t };
			foreach (Type type in t.GetInterfaces())
			{
				allImplementedInterfeces = allImplementedInterfeces.Concat(GetAllInterfaces(type));
			}
			return allImplementedInterfeces;
		}

		private object GetDefaultValue(Type t)
		{
			return t.IsValueType ? Activator.CreateInstance(t) : null;
		}
	}
}

