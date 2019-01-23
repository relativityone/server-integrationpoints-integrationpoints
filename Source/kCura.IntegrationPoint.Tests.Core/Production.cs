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
		private const int _WAIT_TIME_BETWEEN_RETRIES_IN_MILLISECONDS = 1000;

		private static ITestHelper Helper => new TestHelper();

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

				return await productionManager.CreateSingleAsync(workspaceId, production).ConfigureAwait(false);
			}
		}

		public static Task<bool> StageAndWaitForCompletionAsync(int workspaceID, int productionID)
		{
			Func<IProductionManager, int, int, Task<ProductionJobResult>> stageProduction =
				(productionManager, wkspId, prodId) => productionManager.StageProductionAsync(wkspId, prodId);

			return ExecuteAndWaitForCompletionAsync(workspaceID, productionID, stageProduction, "Staged");
		}

		public static Task<bool> RunAndWaitForCompletionAsync(int workspaceID, int productionID)
		{
			Func<IProductionManager, int, int, Task<ProductionJobResult>> runProduction =
				(productionManager, wkspId, prodId) => productionManager.RunProductionAsync(wkspId, prodId, suppressWarnings: true);

			return ExecuteAndWaitForCompletionAsync(workspaceID, productionID, runProduction, "Produced");
		}

		private static async Task<bool> ExecuteAndWaitForCompletionAsync(
			int workspaceID,
			int productionID,
			Func<IProductionManager,int, int, Task<ProductionJobResult>> functionToExecute,
			string expectedStatus)
		{
			using (var productionManager = Helper.CreateAdminProxy<IProductionManager>())
			{
				ProductionJobResult result = await functionToExecute(productionManager, workspaceID, productionID).ConfigureAwait(false);
				if (!result.WasJobCreated)
				{
					return false;
				}
			}
			await WaitForProductionStatusAsync(workspaceID, productionID, expectedStatus).ConfigureAwait(false);
			return true;
		}

		private static async Task WaitForProductionStatusAsync(int workspaceId, int productionId, string expectedStatus, int retriesCount = _MAX_RETRIES_COUNT)
		{
			TimeSpan waitTimeBetweenRetries = TimeSpan.FromMilliseconds(_WAIT_TIME_BETWEEN_RETRIES_IN_MILLISECONDS);

			string status = string.Empty;
			for (var i = 0; i < retriesCount; i++)
			{
				status = await GetProductionStatusAsync(workspaceId, productionId).ConfigureAwait(false);

				if (status == expectedStatus)
				{
					return;
				}

				if (HasErrors(status))
				{
					throw new Exception("ProductionOperation finished with errors");
				}

				await Task.Delay(waitTimeBetweenRetries).ConfigureAwait(false);
			}

			throw new Exception($"ProductionOperation finished with different status than expected. Received {status} expected {expectedStatus}. WorkspaceId={workspaceId}");
		}

		private static bool HasErrors(string status)
		{
			return status.Contains("Error");
		}

		private static async Task<string> GetProductionStatusAsync(int workspaceId, int productionId)
		{
			using (var productionManager = Helper.CreateAdminProxy<IProductionManager>())
			{
				global::Relativity.Productions.Services.Production result = await productionManager.ReadSingleAsync(workspaceId, productionId).ConfigureAwait(false);
				return result.ProductionMetadata.Status.ToString();
			}
		}
	}
}