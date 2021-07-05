using System;

namespace kCura.IntegrationPoints.Core.Contracts.Entity
{
	public static class ObjectTypeGuids
	{
		public static Guid Entity => Guid.Parse(@"d216472d-a1aa-4965-8b36-367d43d4e64c");
	}

	public static class EntityFieldNames
	{
		public const string UniqueId = "Unique ID";
		public const string FirstName = "First Name";
		public const string LastName = "Last Name";
		public const string FullName = "Full Name";
		public const string Manager = "Manager";
	}

	public static class EntityFieldGuids
	{
		public const string FullName = @"57928ef5-f29d-4137-a215-3a9abf3e3f82";
		public const string Respondents = @"6946d84b-7043-488b-b08c-4b41a4de1f8b";
		public const string Notes = @"08bc4e08-a955-4e87-b648-c0a33e40b7a4";
		public const string Email = @"fd825796-2143-4817-9467-11589295cd04";
		public const string Domain = @"a92c1f70-b11f-4c55-8165-0f93dcc1d308";
		public const string FirstName = @"34ee9d29-44bd-4fc5-8ff1-4335a826a07d";
		public const string LastName = @"0b846e7a-6e05-4544-b5a8-ad78c49d0257";
		public const string Manager = @"80bd28d7-dcfb-42d8-bb85-39e4af0051d2";
		public const string UniqueID = @"3c5f8ef5-4ed9-40be-b404-1c70318b3563";

		public static readonly Guid UniqueIdGuid = new Guid(UniqueID);
		public static readonly Guid FirstNameGuid = new Guid(FirstName);
		public static readonly Guid LastNameGuid = new Guid(LastName);
		public static readonly Guid FullNameGuid = new Guid(FullName);
		public static readonly Guid ManagerGuid = new Guid(Manager);
	}
}
