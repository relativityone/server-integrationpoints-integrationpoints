using System;
using System.Threading.Tasks;
using Relativity.DataTransfer.MessageService;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services
{
	public class FakeMessageService : IMessageService
	{
		public Task Send<T>(T message) where T : class, IMessage
		{
			return Task.CompletedTask;
		}

		public Guid Subscribe<T>(Action<T> messageAction) where T : class, IMessage
		{
			return Guid.NewGuid();
		}

		public void Unsubscribe(Guid subscriptionToken)
		{
			
		}
	}
}