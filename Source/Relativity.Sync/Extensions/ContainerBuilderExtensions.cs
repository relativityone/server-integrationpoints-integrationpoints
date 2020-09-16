﻿using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.Sync.Extensions
{
	internal static class ContainerBuilderExtensions
	{
		public static void RegisterTypesInExecutingAssembly<T>(this ContainerBuilder builder)
			where T: class
		{
			builder.RegisterTypes(Assembly.GetExecutingAssembly().GetTypes()
				.Where(t => !t.IsAbstract && t.IsAssignableTo<T>())
				.ToArray()).As<T>();
		}
	}
}
