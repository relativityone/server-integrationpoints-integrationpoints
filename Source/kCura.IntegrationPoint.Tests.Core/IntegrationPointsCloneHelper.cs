using kCura.IntegrationPoints.Data.Repositories.Implementations;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.Services.Objects.DataContracts;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Transformers;
using Relativity.Services.Objects;
using LanguageExt;

namespace kCura.IntegrationPoint.Tests.Core
{
	public class IntegrationPointsCloneHelper
	{
		private const int _MAX_BATCH_SIZE = 1000;
		private const int _INTEGRATION_POINT_ARTIFACT_ID = 1003663;

		private readonly IHelper _helper;
		private readonly IIntegrationPointRepository _repository;

		public IntegrationPointsCloneHelper(IHelper helper, IIntegrationPointRepository repository)
		{
			_helper = helper;
			_repository = repository;
		}

		public async Task<IList<int>> Clone(int workspaceID, int templateIntergationPointArtifactID, int count)
		{
			int lastBatchSize;
			int batchesCount = Math.DivRem(count, _MAX_BATCH_SIZE, out lastBatchSize);

			Stopwatch sw = new Stopwatch();
			sw.Start();
			Debug.WriteLine($"Creating {count} Integration Points has been started...");

			var integrationPoint = await _repository.ReadWithFieldMappingAsync(templateIntergationPointArtifactID).ConfigureAwait(false);

			Debug.WriteLine("Create Integration Points in batches.");
			IList<Task<IList<int>>> batches = new List<Task<IList<int>>>();

			if (lastBatchSize > 0)
			{
				batches.Add(MassCreateIntegrationPoints("Batch_0", workspaceID, integrationPoint, lastBatchSize));
			}

			for (int i = 1; i <= batchesCount; ++i)
			{
				string batchName = $"Batch_{i}";
				batches.Add(MassCreateIntegrationPoints(batchName, workspaceID, integrationPoint, _MAX_BATCH_SIZE));
			}

			var results = await Task.WhenAll(batches).ConfigureAwait(false);

			Debug.WriteLine($"All Integration Points has been created in {sw.ElapsedMilliseconds} miliseconds");

			return results.SelectMany(x => x).ToList();
		}

		private async Task<IList<int>> MassCreateIntegrationPoints(string batchName, int workspaceID,
			IntegrationPoints.Data.IntegrationPoint template, int batchSize)
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();
			Debug.WriteLine($"Creating Integration Points in {batchName} has been started...");
			IEnumerable<IntegrationPoints.Data.IntegrationPoint> integrationPoints = Enumerable.Repeat(template, batchSize);

			MassCreateRequest massCreateRequest = new MassCreateRequest()
			{
				ObjectType = new ObjectTypeRef { Guid = ObjectTypeGuids.IntegrationPointGuid },
				ParentObject = new RelativityObjectRef { ArtifactID = _INTEGRATION_POINT_ARTIFACT_ID },
				Fields = RDOConverter.ConvertPropertiesToFields<IntegrationPoints.Data.IntegrationPoint>().ToList(),
				ValueLists = integrationPoints.Select(
					x => x.ToFieldValues()
						.Select(f => f.Field.Name == "Name" ? Guid.NewGuid().ToString() : f.Value)
						.ToList()
				).ToList()
			};

			using (var proxy = _helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System))
			{
				MassCreateResult result = await proxy.CreateAsync(workspaceID, massCreateRequest).ConfigureAwait(false);
				sw.Stop();
				Debug.WriteLine($"{result.Objects.Count} Integration Points has been created in {batchName} in {sw.Elapsed} miliseconds");

				return result.Objects.Select(x => x.ArtifactID).ToList();
			}
		}
	}
}
