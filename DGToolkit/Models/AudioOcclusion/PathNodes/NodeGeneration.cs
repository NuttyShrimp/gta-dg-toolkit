using System.Collections.Generic;

namespace DGToolkit.Models.AudioOcclusion.PathNodes;

public class NodeGeneration
{
    public int PathListKey { get; set; }
    public int FromRoom { get; set; }
    public int toRoom { get; set; }
    public int hopCount { get; set; }
    public List<int> usedRoom { get; set; }
    public List<NodeGenerationEntry> path { get; set; }
}

public class NodeGenerationEntry
{
    public int PathNodeKey { get; set; }
    public int parent { get; set; }
    public int srcRoom { get; set; }
    public int destRoom { get; set; }
}