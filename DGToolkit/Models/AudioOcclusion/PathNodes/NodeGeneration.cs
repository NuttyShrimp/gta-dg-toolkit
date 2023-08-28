using System.Collections.Generic;
using System.Linq;
using DGToolkit.Models.AudioOcclusion.Metadata;
using DGToolkit.Models.Util;

namespace DGToolkit.Models.AudioOcclusion.PathNodes;

public class NodeGeneration
{
    public int PathListKey { get; set; }
    public int FromRoom { get; set; }
    public int toRoom { get; set; }
    public int hopCount { get; set; }
    public List<int> usedRoom { get; set; }
    public List<List<NodeGenerationEntry>> path { get; set; }

    public bool SearchPath( ref List<int> path, ref List<IndexValue<PortalInfo>> portalInfoList)
    {
        int fromRoom = path[^1];
        if (fromRoom == toRoom)
        {
            return true;
        }

        if (path.Count >= hopCount)
        {
            return false;
        }

        var destRooms = portalInfoList.Where(p =>
                p.Value.DestRoomIdx == toRoom && p.Value.RoomIdx == fromRoom)
            .ToList();

        if (destRooms.Count == 0)
        {
            return false;
        }

        foreach (var hopRoom in destRooms)
        {
            // Skip if in path
            if (path.Contains(hopRoom.Value.DestRoomIdx))
            {
                continue;
            }

            path.Add(hopRoom.Value.DestRoomIdx);
            if (SearchPath(ref path, ref portalInfoList))
            {
                return true;
            }

            path.Remove(hopRoom.Value.DestRoomIdx);
        }

        return false;
    }
}

public class NodeGenerationEntry
{
    public int PathNodeKey { get; set; }
    public int parent { get; set; }
    public int srcRoom { get; set; }
    public int destRoom { get; set; }
}