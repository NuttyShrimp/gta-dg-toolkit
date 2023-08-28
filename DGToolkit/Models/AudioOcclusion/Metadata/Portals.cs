using System;
using System.Collections.Generic;

namespace DGToolkit.Models.AudioOcclusion.Metadata;

public class PortalEntity
{
    public double MaxOcclusion { get; set; }

    // Hash of the entity(cannot be higher/lower than uint32)
    public int EntityModelHashkey { get; set; }
    public bool IsDoor { get; set; }
    public bool IsGlass { get; set; }
}

public class PortalInfo
{
    public Int32 InteriorProxyHash { get; set; }
    public int PortalIdx { get; set; }
    public int RoomIdx { get; set; }
    public Int32 DestInteriorHash { get; set; }
    public int DestRoomIdx { get; set; }
    public List<PortalEntity> PortalEntityList { get; set; }

}