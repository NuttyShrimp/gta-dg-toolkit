using System.Collections.Generic;
using DGToolkit.Models.AudioOcclusion.Xml;

namespace DGToolkit.Models.AudioOcclusion.Graph;

public class RoomGraph
{
    List<RoomNode> roomNodes;

    public RoomGraph()
    {
        roomNodes = new List<RoomNode>();
    }

    public RoomGraph(List<PortalInfo> portals)
    {
        roomNodes = new List<RoomNode>();
        foreach (PortalInfo portal in portals)
        {
            var room1 = GetRoomNode(portal.RoomIdx.value);
            var room2 = GetRoomNode(portal.DestRoomIdx.value);
            if (room1 == null)
            {
                room1 = new RoomNode(portal.RoomIdx.value);
                AddRoomNode(room1);
            }

            if (room2 == null)
            {
                room2 = new RoomNode(portal.DestRoomIdx.value);
                AddRoomNode(room2);
            }

            AddPortal(new Portal(room1, room2, portal));
        }
    }

    public void AddRoomNode(RoomNode roomNode)
    {
        roomNodes.Add(roomNode);
    }

    public void AddPortal(Portal portal)
    {
        RoomNode room1 = portal.Room1;
        RoomNode room2 = portal.Room2;
        room1.AddPortal(portal);
        room2.AddPortal(portal);
    }

    public RoomNode? GetRoomNode(string roomIdx)
    {
        return roomNodes.Find(roomNode => roomNode.RoomIdx == roomIdx);
    }
}