using System.Collections.Generic;

namespace DGToolkit.Models.AudioOcclusion.Graph;

public class RoomNode
{
    public readonly List<Portal> Portals;
    public readonly string RoomIdx;
    
    public RoomNode(string roomIdx)
    {
        RoomIdx = roomIdx;
        Portals = new List<Portal>();
    }

    public void AddPortal(Portal portal)
    {
        Portals.Add(portal);
    }
}