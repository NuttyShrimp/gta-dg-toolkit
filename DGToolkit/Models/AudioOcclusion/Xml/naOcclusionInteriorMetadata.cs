using System.Collections.Generic;
using System.Xml.Serialization;
using static DGToolkit.Models.Util.Util;

namespace DGToolkit.Models.AudioOcclusion.Xml;

public class PortalEntity
{
    // <LinkType value="1" />
    // <MaxOcclusion value="0.7" /><!-- @MaxOcclusion -->
    // <EntityModelHashkey value="997554217" /><!-- @hash_E3674005 -->
    // <IsDoor value="true" />
    // <IsGlass value="true" />
    public Value LinkType = new() {value = "1"};

    // Should be a double between 0 and 1
    public Value MaxOcclusion { get; set; }

    // Hash of the entity(cannot be higher/lower than uint32)
    public Value EntityModelHashkey { get; set; }
    public Value IsDoor { get; set; }
    public Value IsGlass { get; set; }
}

[XmlType("PortalEntityList")]
public class PortalEntityList
{
    [XmlElement("Item")] public List<PortalEntity> PortalEntList { get; set; }
    [XmlAttribute("itemType")] public string itemType = "naOcclusionPortalEntityMetadata";

    public PortalEntityList()
    {
        PortalEntList = new List<PortalEntity>();
    }
}

public class PortalInfo
{
    // <InteriorProxyHash value="1690616543" /><!-- @OcclusionHash -->
    // <PortalIdx value="0" /><!-- @PortalIdx -->
    // <RoomIdx value="0" /><!-- @RoomIdx -->
    // <DestInteriorHash value="1690616543" /><!-- @OcclusionHash -->
    // <DestRoomIdx value="1" /><!-- @DestRoomIdx -->
    public Value InteriorProxyHash { get; set; }
    public Value PortalIdx { get; set; }
    public Value RoomIdx { get; set; }
    public Value DestInteriorHash { get; set; }
    public Value DestRoomIdx { get; set; }
    public PortalEntityList PortalEntityList { get; set; }

    public static PortalInfo FromMetadataPortal(Metadata.PortalInfo info)
    {
        return new PortalInfo()
        {
            PortalIdx = CreateValue(info.PortalIdx.ToString()),
            RoomIdx = CreateValue(info.RoomIdx.ToString()),
            DestRoomIdx = CreateValue(info.DestRoomIdx.ToString()),
            DestInteriorHash = CreateValue(info.DestInteriorHash.ToString()),
            InteriorProxyHash = CreateValue(info.InteriorProxyHash.ToString()),
            PortalEntityList = new PortalEntityList()
            {
                PortalEntList = info.PortalEntityList.ConvertAll(pel =>
                    new PortalEntity()
                    {
                        IsDoor = CreateValue(pel.IsDoor.ToString()),
                        IsGlass = CreateValue(pel.IsGlass.ToString()),
                        MaxOcclusion = CreateValue(pel.MaxOcclusion.ToString()),
                        EntityModelHashkey = CreateValue(pel.EntityModelHashkey.ToString())
                    }
                )
            },
        };
    }
}

public class PortalInfoContainer
{
    [XmlElement("Item")] public List<PortalInfo> PortalInfoList { get; set; }
    [XmlAttribute("itemType")] public string itemType = "naOcclusionPortalInfoMetadata";

    public PortalInfoContainer()
    {
        PortalInfoList = new List<PortalInfo>();
    }
}

public class PathNodeChild
{
    // <PathNodeKey value="0" />
    // <PortalInfoIdx value="15" />
    public Value PathNodeKey { get; set; }
    public Value PortalInfoIdx { get; set; }
}

public class PathNodeChildList
{
    [XmlElement("Item")] public List<PathNodeChild> PortalEntList { get; set; }
    [XmlAttribute("itemType")] public string itemType = "naOcclusionPathNodeChildMetadata";

    public PathNodeChildList()
    {
        PortalEntList = new List<PathNodeChild>();
    }
}

public class PathNode
{
    // <Key value="1568733347" />
    public Value Key { get; set; }
    public PathNodeChildList PathNodeChildList { get; set; }
}

public class PortalNodeContainer
{
    [XmlElement("Item")] public List<PathNode> PortalInfoList { get; set; }
    [XmlAttribute("itemType")] public string itemType = "naOcclusionPathNodeMetadata";

    public PortalNodeContainer()
    {
        PortalInfoList = new List<PathNode>();
    }
}

public class naOcclusionInteriorMetadata
{
    public PortalInfoContainer PortalInfoList;
    public PortalNodeContainer PathNodeList;

    public naOcclusionInteriorMetadata()
    {
        PortalInfoList = new PortalInfoContainer();
        PathNodeList = new PortalNodeContainer();
    }
}