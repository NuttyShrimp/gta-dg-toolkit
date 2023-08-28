using DGToolkit.Models.AudioOcclusion.Xml;

namespace DGToolkit.Models.AudioOcclusion.Graph;

// Represents a undirected edge between two rooms
public class Portal
{
    public readonly RoomNode Room1;
    public readonly RoomNode Room2;
    public readonly PortalInfo PortalInfo;

    public Portal(RoomNode room1, RoomNode room2, PortalInfo portalInfo)
    {
        this.Room1 = room1;
        this.Room2 = room2;
        this.PortalInfo = portalInfo;
    }
}