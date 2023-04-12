using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DGToolkit.Models.Clothing;

public class ClothNameResolver
{
    public Types.ClothTypes ClothType { get; }
    public Types.DrawableTypes DrawableType { get; }
    public string BindedNumber { get; }
    public string Postfix { get; } = "";
    public bool IsVariation { get; }

    public ClothNameResolver(string filename)
    {
        var re = new Regex(@"^.+\^");
        filename = Path.GetFileNameWithoutExtension(filename);
        if (re.IsMatch(filename))
        {
            filename = re.Replace(filename, "");
        }

        string[] parts = filename.Split('_');
        if (parts.Length < 3)
            throw new Exception("Wrong drawable name");

        if (parts[0].ToLower() == "p")
        {
            ClothType = Types.ClothTypes.PedProp;

            string drName = parts[1].ToLower();
            switch (drName)
            {
                case "head":
                    DrawableType = Types.DrawableTypes.PropHead;
                    break;
                case "eyes":
                    DrawableType = Types.DrawableTypes.PropEyes;
                    break;
                case "ears":
                    DrawableType = Types.DrawableTypes.PropEars;
                    break;
                case "mouth":
                    DrawableType = Types.DrawableTypes.PropMouth;
                    break;
                case "lhand":
                    DrawableType = Types.DrawableTypes.PropLHand;
                    break;
                case "rhand":
                    DrawableType = Types.DrawableTypes.PropRHand;
                    break;
                case "lwrist":
                    DrawableType = Types.DrawableTypes.PropLWrist;
                    break;
                case "rwrist":
                    DrawableType = Types.DrawableTypes.PropRWrist;
                    break;
                case "hip":
                    DrawableType = Types.DrawableTypes.PropHip;
                    break;
                case "lfoot":
                    DrawableType = Types.DrawableTypes.PropLFoot;
                    break;
                case "rfoot":
                    DrawableType = Types.DrawableTypes.PropRFoot;
                    break;
                case "unk1":
                    DrawableType = Types.DrawableTypes.PropUnk1;
                    break;
                case "unk2":
                    DrawableType = Types.DrawableTypes.PropUnk2;
                    break;
                default: break;
            }

            BindedNumber = parts[2];
        }
        else
        {
            ClothType = Types.ClothTypes.Component;

            string drName = parts[0].ToLower();
            switch (drName)
            {
                case "head":
                    DrawableType = Types.DrawableTypes.Head;
                    break;
                case "berd":
                    DrawableType = Types.DrawableTypes.Mask;
                    break;
                case "hair":
                    DrawableType = Types.DrawableTypes.Hair;
                    break;
                case "uppr":
                    DrawableType = Types.DrawableTypes.Body;
                    break;
                case "lowr":
                    DrawableType = Types.DrawableTypes.Legs;
                    break;
                case "hand":
                    DrawableType = Types.DrawableTypes.Bag;
                    break;
                case "feet":
                    DrawableType = Types.DrawableTypes.Shoes;
                    break;
                case "teef":
                    DrawableType = Types.DrawableTypes.Accessories;
                    break;
                case "accs":
                    DrawableType = Types.DrawableTypes.Undershirt;
                    break;
                case "task":
                    DrawableType = Types.DrawableTypes.Armor;
                    break;
                case "decl":
                    DrawableType = Types.DrawableTypes.Decal;
                    break;
                case "jbib":
                    DrawableType = Types.DrawableTypes.Top;
                    break;
                default: break;
            }

            BindedNumber = parts[1];
            Postfix = parts[2].ToLower();
            if (parts.Length > 3)
                IsVariation = true;
        }
    }

    public override string ToString()
    {
        return ClothType + " " + DrawableType + " " + BindedNumber;
    }

    public static Types.DrawableTypes TypeIdToDrawableType(Types.ClothTypes clothType, int id)
    {
        if (clothType == Types.ClothTypes.PedProp)
        {
            return Types.DrawableTypes.PropHead + id;
        }

        return Types.DrawableTypes.Head + id;
    }

    public static string DrawableTypeToString(Types.DrawableTypes types)
    {
        switch (types)
        {
            case Types.DrawableTypes.Head: return "head";
            case Types.DrawableTypes.Mask: return "berd";
            case Types.DrawableTypes.Hair: return "hair";
            case Types.DrawableTypes.Body: return "uppr";
            case Types.DrawableTypes.Legs: return "lowr";
            case Types.DrawableTypes.Bag: return "hand";
            case Types.DrawableTypes.Shoes: return "feet";
            case Types.DrawableTypes.Accessories: return "teef";
            case Types.DrawableTypes.Undershirt: return "accs";
            case Types.DrawableTypes.Armor: return "task";
            case Types.DrawableTypes.Decal: return "decl";
            case Types.DrawableTypes.Top: return "jbib";
            case Types.DrawableTypes.PropHead: return "p_head";
            case Types.DrawableTypes.PropEyes: return "p_eyes";
            case Types.DrawableTypes.PropEars: return "p_ears";
            case Types.DrawableTypes.PropMouth: return "p_mouth";
            case Types.DrawableTypes.PropLHand: return "p_lhand";
            case Types.DrawableTypes.PropRHand: return "p_rhand";
            case Types.DrawableTypes.PropLWrist: return "p_lwrist";
            case Types.DrawableTypes.PropRWrist: return "p_rwrist";
            case Types.DrawableTypes.PropHip: return "p_hip";
            case Types.DrawableTypes.PropLFoot: return "p_lfoot";
            case Types.DrawableTypes.PropRFoot: return "p_rfoot";
            case Types.DrawableTypes.PropUnk1: return "p_unk1";
            case Types.DrawableTypes.PropUnk2: return "p_unk2";
            default: return "";
        }
    }
}