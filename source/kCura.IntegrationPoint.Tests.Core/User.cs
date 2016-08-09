﻿using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

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
				Password = "Test1234!",
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

		public static UserModel ReadUser(int userArtifactId)
		{
			string url = $"Relativity/User/{ userArtifactId }";
			string response = Rest.GetRequest(url, false, SharedVariables.RelativityUserName, SharedVariables.RelativityPassword);
			JObject userJObject = JObject.Parse(response);
			UserModel userModel = userJObject.ToObject<UserModel>();
			return userModel;
		}

		public static UserModel ReadUser(string email)
		{
			string url = $"Relativity/User/QueryResult";
			string QueryInputJSON = string.Format(@"{{""condition"":"" 'Email Address' == '{0}'"", ""fields"":[""*""]}}", email);
			string response = Rest.PostRequestAsJson(url, false, QueryInputJSON);
			JObject userJObject = JObject.Parse(response);
			UserModel userModel = userJObject.ToObject<UserModel>();
			return userModel;
		}
	}
}