﻿using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.ImportProvider.Parser.FileIdentification;
using kCura.IntegrationPoints.ImportProvider.Parser.FileIdentification.OutsideInServices;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;
using kCura.IntegrationPoints.ImportProvider.Parser.Services;
using kCura.IntegrationPoints.ImportProvider.Parser.Services.Interfaces;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Installers
{
    public class ServicesInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<IFieldParserFactory>().ImplementedBy<FieldParserFactory>().LifestyleTransient().OnlyNewServices());
            container.Register(Component.For<IWinEddsBasicLoadFileFactory>().ImplementedBy<WinEddsBasicLoadFileFactory>().LifestyleTransient().OnlyNewServices());
            container.Register(Component.For<IWinEddsLoadFileFactory>().ImplementedBy<WinEddsLoadFileFactory>().LifestyleTransient().OnlyNewServices());
            container.Register(Component.For<IWinEddsFileReaderFactory>().ImplementedBy<WinEddsFileReaderFactory>().LifestyleSingleton().OnlyNewServices());
            container.Register(Component.For<IDataReaderFactory>().ImplementedBy<DataReaderFactory>().LifestyleTransient().OnlyNewServices());
            container.Register(Component.For<IImportPreviewService>().ImplementedBy<ImportPreviewService>().LifestyleTransient());
            container.Register(Component.For<IPreviewJobFactory>().ImplementedBy<PreviewJobFactory>().LifestyleTransient());
            container.Register(Component.For<IImportFileLocationService>().ImplementedBy<ImportFileLocationService>().LifestyleTransient());
            container.Register(Component.For<IWebApiConfig>().ImplementedBy<WebApiConfig>().LifestyleSingleton());

            InstallFileIdentificationComponents(container);
        }

        public void InstallFileIdentificationComponents(IWindsorContainer container)
        {
            container.Register(Component.For<IExporterFactory>().ImplementedBy<ExporterFactory>().LifestyleTransient());
            container.Register(Component.For<IOutsideInService>().ImplementedBy<OutsideInService>().LifestyleTransient());
            container.Register(Component.For<IFileIdentificationService>().ImplementedBy<FileIdentificationService>().LifestyleTransient());

            var fileMetadataStore = new FileMetadataStore();
            container.Register(Component.For<IReadOnlyFileMetadataStore>().Instance(fileMetadataStore).LifestyleSingleton());
            container.Register(Component.For<IFileMetadataCollector>().Instance(fileMetadataStore).LifestyleSingleton());
        }
    }
}
