using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CodeWalker.GameFiles;
using DGToolkit.Models.Clothing.Options;

namespace DGToolkit.Models.Clothing.YMT;

public class Loader
{
    public static string GetDrawableFileName(string dlcName, int index, Types.DrawableTypes type, string postfix)
    {
        return
            $"{dlcName}^{ClothNameResolver.DrawableTypeToString(type)}_{index.ToString().PadLeft(3, '0')}{postfix}.ydd";
    }

    public static string GetTextureFileName(string dlcName, int compIdx, int texIdx, Types.DrawableTypes type,
        string postfix)
    {
        return
            $"{dlcName}^{ClothNameResolver.DrawableTypeToString(type)}_diff_{compIdx.ToString().PadLeft(3, '0')}_{(char) ('a' + texIdx)}{postfix}.ytd";
    }

    public static List<ClothData> LoadForDLC(ShopPedApparel dlcInfo)
    {
        // Search the ymt
        var ymtLocation = Path.Combine(ClothingStore.Instance.Options.data.ResourceFolder, "stream",
            $"{dlcInfo.fullDlcName}.ymt");
        var ymtBytes = File.ReadAllBytes(ymtLocation);
        PedFile ymt = new PedFile();
        RpfFile.LoadResourceFile<PedFile>(ymt, ymtBytes, 2);
        string xml = MetaXml.GetXml(ymt.Meta);

        // Parse the xml to XML.Root with serialization
        using var reader = new StringReader(xml);
        var xmlRoot =
            System.Xml.Serialization.XmlSerializer.FromTypes(new[] {typeof(XML.Root)})[0]
                .Deserialize(reader) as XML.Root;

        List<ClothData> drawableList = new();

        int compId = 0;
        List<Types.DrawableTypes> usedDrawables = new();
        foreach (var comp in xmlRoot.availComp.Split(" "))
        {
            if (comp != "255")
            {
                usedDrawables.Add(ClothNameResolver.TypeIdToDrawableType(Types.ClothTypes.Component, compId));
            }

            compId++;
        }

        int compItemIdx = 0;
        foreach (var drawableData in xmlRoot.aComponentData3.drawableList)
        {
            if (compItemIdx >= usedDrawables.Count)
            {
                break;
            }

            for (var i = 0; i < drawableData.aDrawblData3.Drawables.Length; i++)
            {
                var drawable = drawableData.aDrawblData3.Drawables[i];
                var clothData = new ClothData(dlcInfo.fullDlcName, Types.ClothTypes.Component,
                    usedDrawables[compItemIdx], i, drawable.propMask.value == "17" ? "_r" : "_u", "");
                clothData.PropMask = int.Parse(drawable.propMask.value);
                for (var j = 0; j < drawable.aTexData.Item.Length; j++)
                {
                    clothData.Textures.Add(new TextureData()
                    {
                        FileName = GetTextureFileName(
                            dlcInfo.fullDlcName,
                            clothData.CurrentComponentIndex, j, clothData.DrawableType,
                            clothData.IsPostfix_U() ? "_uni" : "_whi"),
                        OffsetLetter = (char) (ClothData.OffsetLetter + j),
                    });
                }

                clothData.ValidateFileExisting();
                drawableList.Add(clothData);
            }

            compItemIdx++;
        }

        foreach (var propData in xmlRoot.propInfo.aPropMetaData.Item)
        {
            var drawData = new ClothData(dlcInfo.fullDlcName, Types.ClothTypes.PedProp,
                ClothNameResolver.TypeIdToDrawableType(Types.ClothTypes.PedProp, int.Parse(propData.anchorId.value)),
                int.Parse(propData.propId.value), "",
                "");
            for (var j = 0; j < propData.ComponentInfo.Item.Length; j++)
            {
                drawData.Textures.Add(new TextureData()
                {
                    FileName = GetTextureFileName(
                        dlcInfo.fullPropsDlcName,
                        drawData.CurrentComponentIndex, j, drawData.DrawableType,
                        ""),
                    OffsetLetter = (char) (ClothData.OffsetLetter + j),
                });
            }

            drawData.ExpressionMods = propData.expressionMods.Split(" ").Select(expr => double.Parse(expr)).ToArray();

            drawData.ValidateFileExisting();
            drawableList.Add(drawData);
        }

        return drawableList;
    }
}