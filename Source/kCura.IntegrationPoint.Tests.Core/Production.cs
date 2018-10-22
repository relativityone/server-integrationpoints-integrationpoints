using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using Relativity.Productions.Services;
using Relativity.Services.Field;
using System;
using System.Threading.Tasks;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class Production
	{
		private const int _MAX_RETRIES_COUNT = 100;
		private const int _WAIT_TIME_BETWEEN_RETRIES_IN_MILISECONDS = 1000;

		public static ITestHelper Helper => new TestHelper();

		public static async Task<int> Create(int workspaceId, string productionName)
		{
			const int productionFontSize = 10;

			using (var productionManager = Helper.CreateAdminProxy<IProductionManager>())
			{
				var production = new global::Relativity.Productions.Services.Production
				{
					Name = productionName,
					ShouldCopyInstanceOnWorkspaceCreate = false,
					Details = new ProductionDetails
					{
						BrandingFontSize = productionFontSize,
						ScaleBrandingFont = false
					},
					Numbering = new DocumentFieldNumbering
					{
						NumberingType = NumberingType.DocumentField,
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

				return await productionManager.CreateSingleAsync(workspaceId, production);
			}
		}

		public static async Task<bool> StageAndWaitForCompletionAsync(int workspaceId, int productionId)
		{
			using (var productionManager = Helper.CreateAdminProxy<IProductionManager>())
			{
				ProductionJobResult result = await productionManager.StageProductionAsync(workspaceId, productionId);
				if (!result.WasJobCreated)
				{
					return false;
				}
			}

			await WaitForProductionStatusAsync(workspaceId, productionId, "Staged");
			return true;
		}

		public static async Task<bool> RunAndWaitForCompletionAsync(int workspaceId, int productionId, bool suppressWarnings = true, bool overrideConflicts = false)
		{
			using (var productionManager = Helper.CreateAdminProxy<IProductionManager>())
			{
				ProductionJobResult result = await productionManager.RunProductionAsync(workspaceId, productionId, suppressWarnings, overrideConflicts);
				if (!result.WasJobCreated)
				{
					return false;
				}
			}
			await WaitForProductionStatusAsync(workspaceId, productionId, "Produced");
			return true;
		}

		private static async Task WaitForProductionStatusAsync(int workspaceId, int productionId, string expectedStatus, int retriesCount = _MAX_RETRIES_COUNT)
		{
			TimeSpan waitTimeBetweenRetries = TimeSpan.FromMilliseconds(_WAIT_TIME_BETWEEN_RETRIES_IN_MILISECONDS);

			string status = string.Empty;
			for (var i = 0; i < retriesCount; i++)
			{
				status = await GetProductionStatusAsync(workspaceId, productionId);

				if (status == expectedStatus)
				{
					return;
				}

				if (status.Contains("Error"))
				{
					throw new Exception("ProductionOperation finished with errors");
				}

				await Task.Delay(waitTimeBetweenRetries);
			}

			throw new Exception($"ProductionOperation finished with different status than expected. Received {status} expected {expectedStatus}. WorkspaceId={workspaceId}");
		}

		private static async Task<string> GetProductionStatusAsync(int workspaceId, int productionId)
		{
			using (var productionManager = Helper.CreateAdminProxy<IProductionManager>())
			{
				global::Relativity.Productions.Services.Production result = await productionManager.ReadSingleAsync(workspaceId, productionId);
				return result.ProductionMetadata.Status.ToString();
			}
		}
	}
}