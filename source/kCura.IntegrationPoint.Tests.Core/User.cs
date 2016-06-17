using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class User
	{
		public static UserModel CreateUser(string firstName, string lastName, string emailAddress, IList<int> groupIds = null)
		{
			List<BaseField> groups = new List<BaseField>();

			if (groupIds == null)
			{
				groups.Add(new BaseField { ArtifactId = 20 }); // System Administrators
			}
			else
			{
				foreach (int groupId in groupIds)
				{
					groups.Add(new BaseField { ArtifactId = groupId });
				}
			}

			UserModel user = new UserModel
			{
				ArtifactTypeId = 2,
				ArtifactTypeName = "User",
				ParentArtifact = new BaseField { ArtifactId = 20 },
				Groups = groups.ToArray(),
				FirstName = firstName,
				LastName = lastName,
				EmailAddress = emailAddress,
				Type = new BaseFields
				{
					ArtifactId = 663,
					ArtifactTypeId = 7,
					ArtifactTypeName = "Choice"
				},
				ItemListPageLength = 25,
				Client = new BaseFields
				{
					ArtifactId = 1006066,
					ArtifactTypeId = 5,
					ArtifactTypeName = "Client"
				},
				AuthenticationData = String.Empty,
				DefaultSelectedFileType = new BaseFields
				{
					ArtifactId = 1014420,
					ArtifactTypeId = 7,
					ArtifactTypeName = "Choice"
				},
				BetaUser = false,
				ChangeSettings = true,
				TrustedIPs = String.Empty,
				RelativityAccess = true,
				AdvancedSearchPublicByDefault = false,
				NativeViewerCacheAhead = true,
				ChangePassword = true,
				MaximumPasswordAge = 0,
				ChangePasswordNextLogin = false,
				SendPasswordTo = new BaseFields
				{
					ArtifactId = 1015049,
					ArtifactTypeId = 7,
					ArtifactTypeName = "Choice"
				},
				PasswordAction = new BaseFields
				{
					ArtifactId = 1015048,
					ArtifactTypeId = 7,
					ArtifactTypeName = "Choice"
				},
				//Password = "Test1234!",
				DocumentSkip = new BaseFields
				{
					ArtifactId = 1015042,
					ArtifactTypeId = 7,
					ArtifactTypeName = "Choice"
				},
				DataFocus = 1,
				KeyboardShortcuts = true,
				EnforceViewerCompatibility = true,
				SkipDefaultPreference = new BaseFields
				{
					ArtifactId = 1015044,
					ArtifactTypeId = 7,
					ArtifactTypeName = "Choice"
				}
			};

			string parameters = JsonConvert.SerializeObject(user);
			string response = Rest.PostRequestAsJson("Relativity/User", false, parameters);
			JObject resultObject = JObject.Parse(response);
			user.ArtifactId = resultObject["Results"][0]["ArtifactID"].Value<int>();
			return user;
		}

		public static void DeleteUser(int userArtifactId)
		{
			if (userArtifactId != 0)
			{
				Rest.DeleteRequestAsJson(SharedVariables.TargetHost, $"Relativity/User/{userArtifactId}", SharedVariables.RelativityUserName, SharedVariables.RelativityPassword, false);
			}
		}

		#region Deprecated RSAPI

		//public static bool CreateUser(string firstName, string lastName, string emailAddress)
		//{
		//	using (IRSAPIClient rsapiClient = _helper.Rsapi.CreateRsapiClient())
		//	{
		//		rsapiClient.APIOptions.WorkspaceID = -1;

		//		int defaultSelectedFileType = 1;
		//		int userType = 3;
		//		int documentSkip = 1000003;
		//		int skipDefaultPreference = 1000004;
		//		int password = 1000005;
		//		int sendNewPasswordTo = 1000006;

		//		// STEP 1: Get the ArtifactIDs for the required Choice, Group, and Client objects.
		//		int returnPasswordCodeId = FindChoiceArtifactId(rsapiClient, sendNewPasswordTo, "Return");
		//		int passwordCodeId = FindChoiceArtifactId(rsapiClient, password, "Auto-generate password");
		//		int documentSkipCodeId = FindChoiceArtifactId(rsapiClient, documentSkip, "Enabled");

		//		int documentSkipPreferenceCodeId = FindChoiceArtifactId(rsapiClient, skipDefaultPreference, "Normal");
		//		int defaultFileTypeCodeId = FindChoiceArtifactId(rsapiClient, defaultSelectedFileType, "Native");
		//		int userTypeCodeId = FindChoiceArtifactId(rsapiClient, userType, "Internal");

		//		int everyoneGroupArtifactId = FindGroupArtifactId(rsapiClient, "Everyone");
		//		int clientArtifactId = FindClientArtifactId(rsapiClient, "Relativity Template");

		//		// STEP 2: Create a User DTO for the User that you want to create.
		//		kCura.Relativity.Client.DTOs.User userDto = new kCura.Relativity.Client.DTOs.User
		//		{
		//			AdvancedSearchPublicByDefault = true,
		//			AuthenticationData = "",
		//			BetaUser = false,
		//			ChangePassword = true,
		//			ChangePasswordNextLogin = false,
		//			ChangeSettings = true,
		//			Client = new kCura.Relativity.Client.DTOs.Client(clientArtifactId),
		//			DataFocus = 1,
		//			DefaultSelectedFileType = new kCura.Relativity.Client.DTOs.Choice(defaultFileTypeCodeId),
		//			DocumentSkip = new kCura.Relativity.Client.DTOs.Choice(documentSkipCodeId),
		//			EmailAddress = emailAddress,
		//			EnforceViewerCompatibility = true,
		//			FirstName = firstName,
		//			Groups =
		//				new List<kCura.Relativity.Client.DTOs.Group> {new kCura.Relativity.Client.DTOs.Group(everyoneGroupArtifactId)},
		//			ItemListPageLength = 25,
		//			KeyboardShortcuts = true,
		//			LastName = lastName,
		//			MaximumPasswordAge = 0,
		//			NativeViewerCacheAhead = true,
		//			PasswordAction = new kCura.Relativity.Client.DTOs.Choice(passwordCodeId),
		//			RelativityAccess = true,
		//			SendPasswordTo = new kCura.Relativity.Client.DTOs.Choice(returnPasswordCodeId),
		//			SkipDefaultPreference = new kCura.Relativity.Client.DTOs.Choice(documentSkipPreferenceCodeId),
		//			TrustedIPs = "",
		//			Type = new kCura.Relativity.Client.DTOs.Choice(userTypeCodeId)
		//		};

		//		WriteResultSet<kCura.Relativity.Client.DTOs.User> createResults;

		//		// STEP 3: Attempt to create the User.
		//		try
		//		{
		//			createResults = rsapiClient.Repositories.User.Create(userDto);
		//		}
		//		catch (Exception ex)
		//		{
		//			Console.WriteLine("An error occurred: {0}", ex.Message);
		//			return false;
		//		}

		//		// Check for success.
		//		if (!createResults.Success)
		//		{
		//			Console.WriteLine("An error occurred creating user: {0}", createResults.Message);

		//			foreach (Result<kCura.Relativity.Client.DTOs.User> createResult in createResults.Results)
		//			{
		//				if (!createResult.Success)
		//				{
		//					Console.WriteLine("   An error occurred in create request: {0}", createResult.Message);
		//				}
		//			}
		//			return false;
		//		}

		//		//STEP 4: Output the password.
		//		Console.WriteLine("Password for created user is {0}", createResults.Results[0].Artifact["Password"]);

		//		return true;
		//	}
		//}

		//private static int FindChoiceArtifactId(IRSAPIClient proxy, int choiceType, string value)
		//{
		//	int artifactId = 0;
		//	WholeNumberCondition choiceTypeCondition = new WholeNumberCondition(ChoiceFieldNames.ChoiceTypeID, NumericConditionEnum.EqualTo, choiceType);
		//	TextCondition choiceNameCondition = new TextCondition(ChoiceFieldNames.Name, TextConditionEnum.EqualTo, value);
		//	CompositeCondition choiceCompositeCondition = new CompositeCondition(choiceTypeCondition, CompositeConditionEnum.And, choiceNameCondition);

		//	Query<kCura.Relativity.Client.DTOs.Choice> choiceQuery = new Query<kCura.Relativity.Client.DTOs.Choice>(new List<FieldValue> { new FieldValue(ArtifactQueryFieldNames.ArtifactID) }, choiceCompositeCondition, new List<Sort>());

		//	try
		//	{
		//		QueryResultSet<kCura.Relativity.Client.DTOs.Choice> choiceQueryResult = proxy.Repositories.Choice.Query(choiceQuery);

		//		if (choiceQueryResult.Success && choiceQueryResult.Results.Count == 1)
		//		{
		//			artifactId = choiceQueryResult.Results.FirstOrDefault().Artifact.ArtifactID;
		//		}
		//		else
		//		{
		//			Console.WriteLine("The choice could not be found.");
		//		}
		//	}
		//	catch (Exception ex)
		//	{
		//		Console.WriteLine("An error occurred: {0}", ex.Message);
		//	}
		//	return artifactId;
		//}

		//private static int FindGroupArtifactId(IRSAPIClient proxy, string group)
		//{
		//	int artifactId = 0;
		//	TextCondition groupCondition = new TextCondition(GroupFieldNames.Name, TextConditionEnum.EqualTo, group);

		//	Query<kCura.Relativity.Client.DTOs.Group> queryGroup = new kCura.Relativity.Client.DTOs.Query<kCura.Relativity.Client.DTOs.Group> { Condition = groupCondition };
		//	queryGroup.Fields.Add(new FieldValue(ArtifactQueryFieldNames.ArtifactID));

		//	try
		//	{
		//		QueryResultSet<kCura.Relativity.Client.DTOs.Group> resultSetGroup = proxy.Repositories.Group.Query(queryGroup, 0);

		//		if (resultSetGroup.Success && resultSetGroup.Results.Count == 1)
		//		{
		//			artifactId = resultSetGroup.Results.FirstOrDefault().Artifact.ArtifactID;
		//		}
		//		else
		//		{
		//			Console.WriteLine("The Query operation failed.{0}{1}", Environment.NewLine, resultSetGroup.Message);
		//		}
		//	}
		//	catch (Exception ex)
		//	{
		//		Console.WriteLine("An error occurred: {0}", ex.Message);
		//	}
		//	return artifactId;
		//}

		//private static int FindClientArtifactId(IRSAPIClient proxy, string group)
		//{
		//	int artifactId = 0;
		//	TextCondition clientCondition = new TextCondition(ClientFieldNames.Name, TextConditionEnum.EqualTo, group);

		//	Query<kCura.Relativity.Client.DTOs.Client> queryClient = new Query<kCura.Relativity.Client.DTOs.Client>
		//	{
		//		Condition = clientCondition,
		//		Fields = FieldValue.AllFields
		//	};

		//	try
		//	{
		//		QueryResultSet<kCura.Relativity.Client.DTOs.Client> resultSetClient = proxy.Repositories.Client.Query(queryClient, 0);

		//		if (resultSetClient.Success && resultSetClient.Results.Count == 1)
		//		{
		//			artifactId = resultSetClient.Results.FirstOrDefault().Artifact.ArtifactID;
		//		}
		//		else
		//		{
		//			Console.WriteLine("The Query operation failed.{0}{1}", Environment.NewLine, resultSetClient.Message);
		//		}
		//	}
		//	catch (Exception ex)
		//	{
		//		Console.WriteLine("An error occurred: {0}", ex.Message);
		//	}
		//	return artifactId;
		//}

		#endregion Deprecated RSAPI
	}
}