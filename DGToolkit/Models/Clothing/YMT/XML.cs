using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DGToolkit.Models.Clothing.YMT;

using DGToolkit.Models.Util;

public class XML
{
    [XmlRoot("CPedVariationInfo")]
    public class Root
    {
        [XmlAttribute] public string name = "";
        public Util.Value bHasTexVariations = Util.CreateValue("false");
        public Util.Value bHasDrawblVariations = Util.CreateValue("false");
        public Util.Value bHasLowLODs = Util.CreateValue("false");
        public Util.Value bIsSuperLOD = Util.CreateValue("false");
        public List<int> availComp = new List<int>() {255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255};

        public ComponentDataList aComponentData3 = new ComponentDataList();
        public SelectionSets aSelectionSets = new SelectionSets();
        public ComponentInfoList compInfos = new ComponentInfoList();
        public PropInfo propInfo = new PropInfo();

        // Joaat of name
        public string dlcName = "";
    }

    public class SelectionSets
    {
        [XmlAttribute] public string itemType = "CPedSelectionSet";
    }

    [XmlType("aComponentData3")]
    public class ComponentDataList
    {
        [XmlAttribute] public string itemType = "CPVComponentData";
        [XmlElement("Item")] public ComponentData[] aDrawblData3 = Array.Empty<ComponentData>();
    }

    public class ComponentData
    {
        public Util.Value numAvailTex = Util.CreateValue("0");
        public DrawableData[] DrawableDatas = new DrawableData[0];
    }

    [XmlType("aDrawblData3")]
    public class DrawableDataList
    {
        [XmlAttribute] public string itemType = "CPVDrawblData";
        [XmlElement("Item")] public DrawableData[] Item = Array.Empty<DrawableData>();
    }

    public class DrawableData
    {
        public Util.Value propMask = Util.CreateValue("1");
        public Util.Value numAlternatives = Util.CreateValue("0");
        public CompTextureDataList aTexData = new CompTextureDataList();
        public ClothData ClothData = new ClothData();
    }

    [XmlType("aTexData")]
    public class CompTextureDataList
    {
        [XmlAttribute] public string itemType = "CPVTextureData";
        [XmlElement("Item")] public CompTextureData[] Item = Array.Empty<CompTextureData>();
    }

    public class CompTextureData
    {
        public Util.Value texId = Util.CreateValue("0");
        public Util.Value distribution = Util.CreateValue("255");
    }

    public class ClothData
    {
        public Util.Value ownsCloth = Util.CreateValue("false");
    }

    [XmlType("compInfos")]
    public class ComponentInfoList
    {
        [XmlAttribute] public string itemType = "CComponentInfo";
        [XmlElement("Item")] public ComponentInfo[] Item = Array.Empty<ComponentInfo>();
    }

    public class ComponentInfo
    {
        public string hash_2FD08CEF = "none";
        public string hash_FC507D28 = "none";
        public int[] hash_07AE529D = new int[] {0, 0, 0, 0, 0};
        public Util.Value flags = Util.CreateValue("0");
        public int inclusions = 0;
        public int exclusions = 0;
        public string hash_6032815C = "PV_COMP_HEAD";
        public Util.Value hash_7E103C8B = Util.CreateValue("0");
        public Util.Value hash_D12F579D = Util.CreateValue("1");
        public Util.Value hash_FA1F27BF = Util.CreateValue("0");
    }

    public class PropInfo
    {
        public Util.Value numAvailProps = Util.CreateValue("0");
        public PropMetadataList aPropMetaData = new PropMetadataList();
    }

    [XmlType("aPropMetaData")]
    public class PropMetadataList
    {
        [XmlAttribute] public string itemType = "CPedPropMetaData";
        [XmlElement("Item")] public PropMetadata[] Item = Array.Empty<PropMetadata>();
    }

    public class PropMetadata
    {
        public string audioId = "none";
        public int[] expressionMods = new int[] {0, 0, 0, 0, 0};
        public PropTextureDataList ComponentInfo = new PropTextureDataList();
        public string renderFlags = "";
        public Util.Value propFlags = Util.CreateValue("0");
        public Util.Value flags = Util.CreateValue("0");
        public Util.Value anchorId = Util.CreateValue("0");
        public Util.Value propId = Util.CreateValue("0");
        public Util.Value hash_AC887A91 = Util.CreateValue("0");
    }

    [XmlType("texData")]
    public class PropTextureDataList
    {
        [XmlAttribute] public string itemType = "CPedPropTexData";
        [XmlElement("Item")] public PropMetadata[] Item = Array.Empty<PropMetadata>();
    }

    public class PropTextureData
    {
        public int inclusions = 0;
        public int exclusions = 0;
        public Util.Value texId = Util.CreateValue("0");
        public Util.Value inclusionId = Util.CreateValue("0");
        public Util.Value exclusionId = Util.CreateValue("0");
        public Util.Value distribution = Util.CreateValue("255");
    }
}