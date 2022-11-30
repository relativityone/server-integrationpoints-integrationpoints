
// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.

#pragma warning disable RG2001 // Character Per Line

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "<Pending>", Scope = "member", Target = "~P:Relativity.Sync.Tests.Integration.Stubs.ConfigurationStub.BatchesIds")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Internal classes here are either instantiated by NUnit (fixtures) or by a container (stubs/mocks)", Scope = "module")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "RG2009:With the exception of zero and one, never hard-code a numeric value; always declare a constant instead", Justification = "Class contains test data, which requires several literal ints", Scope = "type", Target = "~T:Relativity.Sync.Tests.Integration.SourceWorkspaceDataReaderMetadataTests")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:Identifiers should not contain underscores", Justification = "<Pending>", Scope = "module")]

#pragma warning restore RG2001 // Character Per Line
