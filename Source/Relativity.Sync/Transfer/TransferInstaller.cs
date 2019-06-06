﻿using System.Linq;
using System.Reflection;
using Autofac;

namespace Relativity.Sync.Transfer
{
	internal sealed class TransferInstaller : IInstaller
	{
		public void Install(ContainerBuilder builder)
		{
			builder.RegisterType<DocumentFieldRepository>().As<IDocumentFieldRepository>();
			builder.RegisterType<BatchDataReaderBuilder>().As<IBatchDataReaderBuilder>();
			builder.RegisterType<RelativityExportBatcher>().As<IRelativityExportBatcher>();
			builder.RegisterType<NativeFileRepository>().As<INativeFileRepository>();
			builder.RegisterType<FieldManager>().As<IFieldManager>();
			builder.RegisterType<ExportDataSanitizer>().As<IExportDataSanitizer>();
			builder.RegisterType<FolderPathRetriever>().As<IFolderPathRetriever>();
			builder.RegisterType<ItemStatusMonitor>().As<IItemStatusMonitor>();
			builder.RegisterTypes(Assembly.GetExecutingAssembly().GetTypes()
				.Where(t => !t.IsAbstract && t.IsAssignableTo<ISpecialFieldBuilder>())
				.ToArray()).As<ISpecialFieldBuilder>();
			builder.RegisterTypes(Assembly.GetExecutingAssembly().GetTypes()
				.Where(t => !t.IsAbstract && t.IsAssignableTo<IExportFieldSanitizer>())
				.ToArray()).As<IExportFieldSanitizer>();
		}
	}
}
