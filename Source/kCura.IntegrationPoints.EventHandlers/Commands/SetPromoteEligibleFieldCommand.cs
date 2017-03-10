using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
	public class SetPromoteEligibleFieldCommand : ICommand
	{
		private readonly IRSAPIService _rsapiService;

		public SetPromoteEligibleFieldCommand(IRSAPIService rsapiService)
		{
			_rsapiService = rsapiService;
		}

		public void Execute()
		{
			Update<Data.IntegrationPoint>(new Guid(IntegrationPointFieldGuids.PromoteEligible));
			Update<IntegrationPointProfile>(new Guid(IntegrationPointProfileFieldGuids.PromoteEligible));
		}

		private void Update<T>(Guid promoteEligibleGuid) where T : BaseRdo, new()
		{
			List<T> rdos = GetRdosWithoutPromoteEligibleFieldSet<T>(promoteEligibleGuid);
			UpdateRdos(promoteEligibleGuid, rdos);
		}

		private void UpdateRdos<T>(Guid promoteEligibleGuid, List<T> rdos) where T : BaseRdo, new()
		{
			foreach (var rdo in rdos)
			{
				rdo.Rdo[promoteEligibleGuid] = new FieldValue(promoteEligibleGuid, true);
			}
			_rsapiService.GetGenericLibrary<T>().Update(rdos);
		}

		private List<T> GetRdosWithoutPromoteEligibleFieldSet<T>(Guid guid) where T : BaseRdo, new()
		{
			var query = new Query<RDO>
			{
				Condition = new NotCondition(new BooleanCondition(guid, BooleanConditionEnum.IsSet)),
				Fields = FieldValue.AllFields
			};
			return _rsapiService.GetGenericLibrary<T>().Query(query);
		}

		public string SuccessMessage => "Promote Eligible field successfully updated.";
		public string FailureMessage => "Failed to update Promote Eligible field.";
	}
}