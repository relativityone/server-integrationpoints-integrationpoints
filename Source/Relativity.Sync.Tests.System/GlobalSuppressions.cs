
// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.

#pragma warning disable RG2001 // Character Per Line

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Internal classes here are either instantiated by NUnit (fixtures) or by a container (stubs/mocks)", Scope = "module")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA5359:Do Not Disable Certificate Validation", Justification = "<Pending>", Scope = "member", Target = "~M:Relativity.Sync.Tests.System.SystemTestsSetup.SuppressCertificateCheckingIfConfigured")]

#pragma warning restore RG2001 // Character Per Line