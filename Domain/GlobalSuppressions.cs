// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Public IFileManager member, injected via DI and mocked in tests; making it static would remove it from the interface it's called through.", Scope = "member", Target = "~M:Domain.IO.FileManager.CreateOutputFolder(System.String)~System.String")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Public IFileManager member, injected via DI and mocked in tests; making it static would remove it from the interface it's called through.", Scope = "member", Target = "~M:Domain.IO.FileManager.DirectoryExists(System.String)~System.Boolean")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Public IFileManager member, injected via DI and mocked in tests; making it static would remove it from the interface it's called through.", Scope = "member", Target = "~M:Domain.IO.FileManager.GetFilename(System.String)~System.String")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Public IFileManager member, injected via DI and mocked in tests; making it static would remove it from the interface it's called through.", Scope = "member", Target = "~M:Domain.IO.FileManager.ReturnCorrectWordFilePath(System.String,System.String)~System.String")]
