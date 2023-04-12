using System.Collections.Generic;
using System.IO;
using System.Linq;
using DGToolkit.Models.Clothing.Options;

namespace DGToolkit.Models.Clothing.YMT;

public class Loader
{
    private static string GetCompFileName(int index, Types.DrawableTypes type, string postfix)
    {
        var fileName = $"{index.ToString().PadLeft(3, '0')}_{ClothNameResolver.DrawableTypeToString(type)}";
        if (postfix.Trim() != "")
        {
            fileName += $"_{postfix}";
        }

        return fileName;
    }

    public static List<ClothData> GenerateData(ClothingManifest options, string dlcName)
    {
        var clothData = new List<ClothData>();
        if (!options.PedEntries.ContainsKey(dlcName))
        {
            return clothData;
        }

        var dlcInfo = options.PedEntries[dlcName];

        foreach (var clothInfo in dlcInfo)
        {
            var data = new ClothData(
                Path.Combine(dlcName,
                    GetCompFileName(clothInfo.Numeric, clothInfo.DrawableType, clothInfo.PostFix)),
                dlcName,
                clothInfo.ClothType,
                clothInfo.DrawableType, clothInfo.Numeric, clothInfo.PostFix, clothInfo.Description);
            clothData.Add(data);
            if (clothInfo.ClothType == Types.ClothTypes.PedProp)
            {
                data.ExpressionMods = clothInfo.ExpressionMods;
            }

            clothInfo.TextureMap.ToList().ForEach(x => data.Textures.Add(new TextureData()
            {
                file = x.Key,
                name = x.Value
            }));

            data.SearchForTextures(options.ResourceFolder);
        }

        return clothData;
    }
}