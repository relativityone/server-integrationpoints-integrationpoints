﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using Castle.Windsor;

namespace kCura.IntegrationPoints.Web
{
	public class WindsorCompositionRoot : IHttpControllerActivator
	{
		private readonly IWindsorContainer container;

		public WindsorCompositionRoot(IWindsorContainer container)
		{
			this.container = container;
		}

		public IHttpController Create(
				HttpRequestMessage request,
				HttpControllerDescriptor controllerDescriptor,
				Type controllerType)
		{
			var controller =
					(IHttpController)this.container.Resolve(controllerType);

			request.RegisterForDispose(
					new Release(
							() => this.container.Release(controller)));

			return controller;
		}

		private class Release : IDisposable
		{
			private readonly Action release;

			public Release(Action release)
			{
				this.release = release;
			}

			public void Dispose()
			{
				this.release();
			}
		}
	}
}