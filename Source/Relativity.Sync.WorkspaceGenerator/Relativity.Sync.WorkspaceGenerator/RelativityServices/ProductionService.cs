using System;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Productions.Services;
using Relativity.Services.Field;
using Relativity.Services.ServiceProxy;

namespace Relativity.Sync.WorkspaceGenerator.RelativityServices
{
    internal class ProductionService : IProductionService
    {
        private readonly ServiceFactory _servicesFactory;

        public ProductionService(ServiceFactory servicesFactory)
        {
            _servicesFactory = servicesFactory;
        }

        public async Task<int?> GetProductionIdAsync(int workspaceId, string productionName)
        {
            using (var productionManager = _servicesFactory.CreateProxy<IProductionManager>())
            {
                var allProductions = await productionManager.GetAllAsync(workspaceId, DataSourceReadMode.None).ConfigureAwait(false);
                return allProductions.FirstOrDefault(x => x.Name == productionName)?.ArtifactID;
            }
        }

        public async Task<int> CreateProductionAsync(int workspaceId, string productionName)
        {
            var production = new Productions.Services.Production
            {
                Name = productionName,
                Details = new Productions.Services.ProductionDetails
                {
                    BrandingFontSize = 10,
                    ScaleBrandingFont = false
                },
                Numbering = new DocumentFieldNumbering
                {
                    NumberingType = Productions.Services.NumberingType.DocumentField,
                    NumberingField = new FieldRef
                    {
                        ArtifactID = 1003667,
                        ViewFieldID = 0,
                        Name = "Control Number"
                    },
                    AttachmentRelationalField = new FieldRef
                    {
                        ArtifactID = 0,
                        ViewFieldID = 0,
                        Name = ""
                    },
                    BatesPrefix = "PRE",
                    BatesSuffix = "SUF",
                    IncludePageNumbers = false,
                    DocumentNumberPageNumberSeparator = "",
                    NumberOfDigitsForPageNumbering = 0,
                    StartNumberingOnSecondPage = false
                }
            };

            using (var productionManager = _servicesFactory.CreateProxy<IProductionManager>())
            {
                Console.WriteLine($"Creating production {productionName}");
                return await productionManager.CreateSingleAsync(workspaceId, production).ConfigureAwait(false);
            }
        }
    }
}