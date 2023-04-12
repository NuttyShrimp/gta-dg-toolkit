using System.Collections.Generic;
using System.Linq;

namespace DGToolkit.Models.Clothing.Options;

public class ClothingManifest
{
    public string? ResourceFolder { get; set; }

    // Map of DLC name to drawable descriptions
    public Dictionary<string, List<ClothInfo>> PedEntries { get; set; } = new();
}

public class ClothInfo
{
    public int[] ExpressionMods { get; set; }
    public Types.ClothTypes ClothType { get; set; }

    public Types.DrawableTypes DrawableType { get; set; }

    // eg. 000
    public int Numeric { get; set; }
    public string PostFix { get; set; }
    public string Description { get; set; }
    public Dictionary<string, string> TextureMap { get; set; }

    public ClothInfo()
    {
    }

    public ClothInfo(ClothData data)
    {
        ExpressionMods = data.ExpressionMods;
        ClothType = data.ClothType;
        DrawableType = data.DrawableType;
        Numeric = data.CurrentComponentIndex;
        PostFix = data.PostFix;
        Description = data.Description;
        TextureMap = new Dictionary<string, string>();
        data.Textures.ToList().ForEach(x => TextureMap.Add(x.file, x.name ?? ""));
    }
}