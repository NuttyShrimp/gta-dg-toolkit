using System.Collections.Generic;
using static DGToolkit.Models.Clothing.Types.ClothTypes;
using static DGToolkit.Models.Clothing.Types.DrawableTypes;

namespace DGToolkit.Models.Clothing;

public class Types
{
    public enum ClothTypes
    {
        Component,
        PedProp
    }

    public enum DrawableTypes
    {
        Head,
        Mask,
        Hair,
        Body,
        Legs,
        Bag,
        Shoes,
        Accessories,
        Undershirt,
        Armor,
        Decal,
        Top,
        PropHead,
        PropEyes,
        PropEars,
        PropMouth,
        PropLHand,
        PropRHand,
        PropLWrist,
        PropRWrist,
        PropHip,
        PropLFoot,
        PropRFoot,
        PropUnk1,
        PropUnk2,
    }

    public static Dictionary<ClothTypes, string> ClothTypeDescriptions = new()
    {
        {Component, "Component (Shirt, Pants, Torso, etc.)"},
        {PedProp, "Prop (Hats, Glasses, Watches, etc.)"}
    };

    public static Dictionary<ClothTypes, DrawableTypes[]> ClothDrawableTypes = new()
    {
        {
            Component, new[]
            {
                Head,
                Mask,
                Hair,
                Body,
                Legs,
                Bag,
                Shoes,
                Accessories,
                Undershirt,
                Armor,
                Decal,
                Top,
            }
        },
        {
            PedProp, new[]
            {
                PropHead,
                PropEyes,
                PropEars,
                PropMouth,
                PropLHand,
                PropRHand,
                PropLWrist,
                PropRWrist,
                PropHip,
                PropLFoot,
                PropRFoot,
                PropUnk1,
                PropUnk2,
            }
        }
    };
}