using System;
using Moq;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
	public abstract class KeplerStubBase<T> : IKeplerStub<T>
		where T: class, IDisposable
	{
		public Mock<T> Mock { get; }

		public T Object => Mock.Object;

		protected KeplerStubBase()
		{
			Mock = new Mock<T>();
		}
	}
}
