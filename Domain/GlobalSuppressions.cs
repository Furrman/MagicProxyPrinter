// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Avoid using static methods", Scope = "member", Target = "~M:Domain.IO.FileManager.CreateOutputFolder(System.String)~System.String")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Avoid using static methods", Scope = "member", Target = "~M:Domain.IO.FileManager.DirectoryExists(System.String)~System.Boolean")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Avoid using static methods", Scope = "member", Target = "~M:Domain.IO.FileManager.GetFilename(System.String)~System.String")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Avoid using static methods", Scope = "member", Target = "~M:Domain.IO.FileManager.ReturnCorrectWordFilePath(System.String,System.String)~System.String")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Avoid using static methods", Scope = "member", Target = "~M:Domain.Services.ArchidektService.ParseCardsToDeck(System.Collections.Generic.ICollection{Domain.Models.DTO.Archidekt.DeckCardDTO})~System.Collections.Generic.List{Domain.Models.DTO.CardEntryDTO}")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Avoid using static methods", Scope = "member", Target = "~M:Domain.Services.ScryfallService.AddRelatedTokensToCardImages(Domain.Models.DTO.CardEntryDTO,Domain.Models.DTO.Scryfall.CardDataDTO)")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Avoid using static methods", Scope = "member", Target = "~M:Domain.Services.ScryfallService.GetArtSideOnlyCardLink(Domain.Models.DTO.Scryfall.CardDataDTO)~System.Collections.Generic.HashSet{Domain.Models.DTO.CardSideDTO}")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Avoid using static methods", Scope = "member", Target = "~M:Domain.Services.ScryfallService.IsArtCard(Domain.Models.DTO.CardEntryDTO)~System.Boolean")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Avoid using static methods", Scope = "member", Target = "~M:Domain.Services.ScryfallService.IsDualSideCard(Domain.Models.DTO.Scryfall.CardDataDTO)~System.Boolean")]
