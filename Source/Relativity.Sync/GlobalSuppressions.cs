// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.

#pragma warning disable RG2001 // Character Per Line

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:Do not catch general exception types", Justification = "<Pending>", Scope = "module")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1822:Member does not access instance data and can be marked as static", Justification = "Undesired design", Scope = "type", Target = "~T:Relativity.Sync.Configuration.ImportSettingsDto")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "RG0001: Class names should match the name of the file they are in", Justification = "Private class scope", Scope = "type", Target = "~T:Relativity.Sync.Transfer.ItemStatusMonitor")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "RG2002: Avoid putting multiple classes in a single file", Justification = "Private class scope", Scope = "type", Target = "~T:Relativity.Sync.Transfer.ItemStatusMonitor")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "RG0001: Class names should match the name of the file they are in", Justification = "Inner static class in other static class. Both containing constants.", Scope = "type", Target = "~T:Relativity.Sync.Telemetry.TelemetryConstants")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "RG2002: Avoid putting multiple classes in a single file", Justification = "Inner static class in other static class. Both containing constants.", Scope = "type", Target = "~T:Relativity.Sync.Telemetry.TelemetryConstants")]

#pragma warning restore RG2001 // Character Per Line