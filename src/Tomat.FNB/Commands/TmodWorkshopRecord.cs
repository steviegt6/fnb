using System.Collections.Generic;

namespace Tomat.FNB.Commands;

/// <summary>
///     Represents a tModLoader Workshop item.
/// </summary>
/// <param name="ItemId">The ID of the Workshop item.</param>
/// <param name="Items">A collection of mod items.</param>
internal sealed record TmodWorkshopRecord(long ItemId, List<TmodWorkshopItem> Items);

/// <summary>
///     A tModLoader mod version within a tModLoader Workshop item.
/// </summary>
/// <param name="Version">The tModLoader version.</param>
/// <param name="TmodName">The name of the .tmod archive.</param>
/// <param name="FullPath">The full path to the .tmod archive.</param>
internal sealed record TmodWorkshopItem(string? Version, string TmodName, string FullPath);
