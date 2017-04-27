using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Web.Controllers.API;
using Relativity.Services.FieldMapping;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Domain.Managers;

namespace kCura.IntegrationPoints.Web.Tests.Controllers.API
{
	using kCura.IntegrationPoints.Domain.Models;

	public class InstanceFinderControllerTests
	{
		private InstanceFinderController _controller;

		private ICPHelper _helper;

		private IContextContainerFactory _contextContainerFactory;

		private IManagerFactory _managerFactory;

		private IContextContainer _sourceContextContainer;

		private IFederatedInstanceManager _federatedInstanceManager;

		private List<FederatedInstanceDto> _federatedInstanceDtosResult;

		[SetUp]
		public void SetUp()
		{
			_helper = Substitute.For<ICPHelper>();
			_contextContainerFactory = Substitute.For<IContextContainerFactory>();
			_managerFactory = Substitute.For<IManagerFactory>();
			_sourceContextContainer = Substitute.For<IContextContainer>();
			_federatedInstanceManager = Substitute.For<IFederatedInstanceManager>();
			_federatedInstanceDtosResult = new List<FederatedInstanceDto>() { new FederatedInstanceDto() };

			_contextContainerFactory.CreateContextContainer(_helper).Returns(_sourceContextContainer);
			_managerFactory.CreateFederatedInstanceManager(_sourceContextContainer).Returns(_federatedInstanceManager);
			_federatedInstanceManager.RetrieveAll().Returns(_federatedInstanceDtosResult);

			_controller = new InstanceFinderController(_helper, _contextContainerFactory, _managerFactory)
			{
				Configuration = new HttpConfiguration(),
				Request = new HttpRequestMessage()
			};
		}

		[Test]
		public void ItShouldReturnListOfAllFederatedInstances()
		{
			//act
			HttpResponseMessage responseMessage = _controller.Get();
			List<FederatedInstanceDto> result = this.ExtractFederatedInstancesFromResponse(responseMessage);

			//assert
			CollectionAssert.AreEqual(_federatedInstanceDtosResult, result);
		}

		private List<FederatedInstanceDto> ExtractFederatedInstancesFromResponse(HttpResponseMessage response)
		{

			ObjectContent objectContent = response.Content as ObjectContent;
			List<FederatedInstanceDto> result = (List<FederatedInstanceDto>)objectContent?.Value;
			return result;

		}
	}
}