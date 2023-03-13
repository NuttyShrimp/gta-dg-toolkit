using System.Collections.Generic;

namespace DGToolkit.Models.Clothing.Options;

public class PedMapping
{
    // eg. m or f. will result in m_dg_civ
    public string? DlcPrefix { get; set; }
    public string? PedName { get; set; }
    public Dictionary<string, string> DrawableDescriptions { get; set; } = new();
}

public class File
{
    // eg. dg_civ
    public string? DlcName { get; set; }
    public string? StreamFolder { get; set; }
    public Dictionary<string, PedMapping> PedEntries { get; set; } = new();
}