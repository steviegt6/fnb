using System.Collections.Generic;

namespace Tomat.FNB.Commands;

public record TmodWorkshopRecord(long ItemId, List<TmodWorkshopItem> Items);

public record TmodWorkshopItem(string? Version, string TmodName, string FullPath);
