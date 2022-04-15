﻿using System;
 using System.Data;
 using System.Threading.Tasks;
 using System.Web.Services.Protocols;
 using System.Xml;
 using Castle.DynamicProxy;
 using FluentAssertions;
 using kCura.WinEDDS.Service.Export;
 using Moq;
 using NUnit.Framework;
 using Relativity.Sync.Transfer;

 namespace Relativity.Sync.Tests.Unit.Transfer
{
	public sealed class SearchManagerInterceptorTests
	{
		private const int _SOME_CASE_CONTEXT_ARTIFACT_ID = -1;
		private const string _SOME_DOCUMENT_ARTIFACT_IDS = "-1,-2";

		private static readonly ProxyGenerator _proxyGenerator = new ProxyGenerator();

		[Test]
		public void Interceptor_ShouldReturnValue_WhenNoExceptionIsThrown()
		{
			DataSet expectedValue = new DataSet();

			Mock<ISearchManager> searchManagerStub = new Mock<ISearchManager>();
			searchManagerStub.Setup(x => x.RetrieveNativesForSearch(It.IsAny<int>(), It.IsAny<string>())).Returns(expectedValue);

			ISearchManager instance = PrepareSearchManagerProxy(searchManagerStub);

			// ACT
			DataSet actualValue = instance.RetrieveNativesForSearch(_SOME_CASE_CONTEXT_ARTIFACT_ID, _SOME_DOCUMENT_ARTIFACT_IDS);

			// ASSERT
			actualValue.Should().Be(expectedValue);
		}

		[Test]
		public void Interceptor_ShouldChangeInvocationTarget_WhenNeedToReLoginSoapExceptionIsThrown()
		{
			Mock<ISearchManager> originalInvocationTargetMock = new Mock<ISearchManager>();
			originalInvocationTargetMock.Setup(x => x.RetrieveNativesForSearch(It.IsAny<int>(), It.IsAny<string>())).Throws(new SoapException("NeedToReLoginException", new XmlQualifiedName()));

			Mock<ISearchManager> newInvocationTargetMock =  new Mock<ISearchManager>();

			ISearchManager instance = PrepareSearchManagerProxy(originalInvocationTargetMock, newInvocationTargetMock);

			// ACT
			Func<DataSet> invocation = () => instance.RetrieveNativesForSearch(_SOME_CASE_CONTEXT_ARTIFACT_ID, _SOME_DOCUMENT_ARTIFACT_IDS);

			// ASSERT
			invocation.Should().NotThrow();
			originalInvocationTargetMock.Verify(x => x.RetrieveNativesForSearch(It.IsAny<int>(), It.IsAny<string>()), Times.Once());
			newInvocationTargetMock.Verify(x => x.RetrieveNativesForSearch(It.IsAny<int>(), It.IsAny<string>()), Times.Once());
		}

		private ISearchManager PrepareSearchManagerProxy(Mock<ISearchManager> initialSearchManager, Mock<ISearchManager> factorySearchManager = null)
		{
			if (initialSearchManager is null)
			{
				throw new ArgumentNullException(nameof(initialSearchManager));
			}

			factorySearchManager = factorySearchManager ?? new Mock<ISearchManager>();

			Mock<Func<Task<ISearchManager>>> searchManagerFactoryStub = new Mock<Func<Task<ISearchManager>>>();
			searchManagerFactoryStub.Setup(x => x()).Returns(Task.FromResult(factorySearchManager.Object));

			SearchManagerInterceptor searchManagerInterceptor = new SearchManagerInterceptor(searchManagerFactoryStub.Object);

			return _proxyGenerator.CreateInterfaceProxyWithTargetInterface<ISearchManager>(initialSearchManager.Object, searchManagerInterceptor);
		}
	}
}