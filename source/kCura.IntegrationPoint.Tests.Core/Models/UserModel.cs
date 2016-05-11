using Newtonsoft.Json;

namespace kCura.IntegrationPoint.Tests.Core.Models
{
	public class UserModel
	{
		[JsonProperty(PropertyName = "Artifact Type ID")]
		public int ArtifactTypeId;

		[JsonProperty(PropertyName = "Artifact Type Name")]
		public string ArtifactTypeName;

		[JsonProperty(PropertyName = "Parent Artifact")]
		public BaseField BaseField;

		[JsonProperty(PropertyName = "Groups")]
		public BaseField[] Groups;

		[JsonProperty(PropertyName = "First Name")]
		public string FirstName;

		[JsonProperty(PropertyName = "Last Name")]
		public string LastName;

		[JsonProperty(PropertyName = "Email Address")]
		public string EmailAddress;

		[JsonProperty(PropertyName = "Type")]
		public BaseFields Type;

		[JsonProperty(PropertyName = "Item List Page Length")]
		public int ItemListPageLength;

		[JsonProperty(PropertyName = "Client")]
		public BaseFields Client;

		[JsonProperty(PropertyName = "Authentication Data")]
		public string AuthenticationData;

		[JsonProperty(PropertyName = "Default Selected File Type")]
		public BaseFields DefaultSelectedFileType;

		[JsonProperty(PropertyName = "Beta User")]
		public bool BetaUser;

		[JsonProperty(PropertyName = "Change Settings")]
		public bool ChangeSettings;

		[JsonProperty(PropertyName = "Trusted IPs")]
		public string TrustedIPs;

		[JsonProperty(PropertyName = "Relativity Access")]
		public bool RelativityAccess;

		[JsonProperty(PropertyName = "Advanced Search Public By Default")]
		public bool AdvancedSearchPublicByDefault;

		[JsonProperty(PropertyName = "Native Viewer Cache Ahead")]
		public bool NativeViewerCacheAhead;

		[JsonProperty(PropertyName = "Change Password")]
		public bool ChangePassword;

		[JsonProperty(PropertyName = "Maximum Password Age")]
		public int MaximumPasswordAge;

		[JsonProperty(PropertyName = "Change Password Next Login")]
		public bool ChangePasswordNextLogin;

		[JsonProperty(PropertyName = "Send Password To")]
		public BaseFields SendPasswordTo;

		[JsonProperty(PropertyName = "Password Action")]
		public BaseFields PasswordAction;

		[JsonProperty(PropertyName = "Password")]
		public string Password;

		[JsonProperty(PropertyName = "Document Skip")]
		public BaseFields DocumentSkip;

		[JsonProperty(PropertyName = "Data Focus")]
		public int DataFocus;

		[JsonProperty(PropertyName = "Keyboard Shortcuts")]
		public bool KeyboardShortcuts;

		[JsonProperty(PropertyName = "Enforce Viewer Compatibility")]
		public bool EnforceViewerCompatibility;

		[JsonProperty(PropertyName = "Skip Default Preference")]
		public BaseFields SkipDefaultPreference;
	}
}