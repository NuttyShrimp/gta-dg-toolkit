using System.Collections.Generic;

namespace DGToolkit.Models.AudioOcclusion.PathNodes;

public class PathNodeChild
{
    // 0 if direct otherwise link to PortalInfoIdx otherwise link to PathNode.key
    public int Key;
    public int PortalInfoIdx;
}

public class PathNode
{
    public int Key;
    public List<PathNodeChild> EntList;

    public PathNode()
    {
        EntList = new List<PathNodeChild>();
    }
}