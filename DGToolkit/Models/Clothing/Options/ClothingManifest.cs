using System.Collections.Generic;
using System.Linq;
using DGToolkit.Models.Clothing.YMT;
using Newtonsoft.Json;

namespace DGToolkit.Models.Clothing.Options;

public class ClothingManifest
{
    [JsonIgnore]
    public string? ResourceFolder { get; set; }

    // Map of DLC name to drawable descriptions
    public Dictionary<string, List<ClothInfo>> PedEntries { get; set; } = new();
}

public class ClothInfo
{
    public string drawableName;
    public string Description { get; set; }
    public List<TextureData> TextureMap { get; set; }

    public ClothInfo()
    {
    }

    public ClothInfo(ClothData data)
    {
        drawableName = $"{ClothNameResolver.DrawableTypeToString(data.DrawableType)}_{data.ComponentNumerics}{data.PostFix}";
        Description = data.Description;
        TextureMap = data.Textures.ToList();
    }
}