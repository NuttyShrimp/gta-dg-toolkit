using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CodeWalker.GameFiles;
using DGToolkit.Models.AudioOcclusion.Graph;
using DGToolkit.Models.AudioOcclusion.PathNodes;
using DGToolkit.Models.AudioOcclusion.Xml;
using DGToolkit.Models.Util;
using static DGToolkit.Models.Util.Util;
using PortalEntity = DGToolkit.Models.AudioOcclusion.Metadata.PortalEntity;
using PortalInfo = DGToolkit.Models.AudioOcclusion.Metadata.PortalInfo;

namespace DGToolkit.Models.AudioOcclusion;

public class OcclGenerator
{
    private OcclusionModel _model;
    private InteriorEntry Entry;
    private string outputPath;
    private YmapFile ymap;
    private YtypFile ytyp;
    private MloArchetype mloArchetype;
    private int occlusionHash;

    public OcclGenerator(InteriorEntry entry)
    {
        _model = OcclusionModel.instance;
        Entry = entry;
        outputPath = "";
    }

    private List<PortalInfo> GeneratePortalInfoList()
    {
        var portalInfoList = new List<PortalInfo>();

        foreach (var (fromRoom, destRooms) in Entry.Paths)
        {
            Debug.WriteLine($"From Room: {fromRoom} Dest Rooms: {string.Join(",", destRooms)}");
            foreach (var destRoom in destRooms)
            {
                var portalRoomIdx = 0;
                // Each portal with these rooms
                mloArchetype.portals.Where(p =>
                        (p.Data.roomFrom == fromRoom && p.Data.roomTo == destRoom) ||
                        (p.Data.roomTo == fromRoom && p.Data.roomFrom == destRoom))
                    .ToList()
                    .ForEach(mloPortal =>
                    {
                        var portalInfo = new PortalInfo()
                        {
                            PortalIdx = mloPortal.Index,
                            RoomIdx = fromRoom,
                            DestRoomIdx = destRoom,
                            DestInteriorHash = occlusionHash,
                            InteriorProxyHash = occlusionHash,
                            PortalEntityList = new List<PortalEntity>(),
                        };
                        if (mloPortal.AttachedObjects != null && mloPortal.AttachedObjects.Length > 0)
                        {
                            Entry.portals.Where(p =>
                                p.portalRoomIdx == portalRoomIdx && p.srcRoomIdx == fromRoom &&
                                p.destRoomIdx == destRoom).ToList().ForEach(rp =>
                            {
                                var portalEntity = new PortalEntity()
                                {
                                    EntityModelHashkey = rp.entityHash,
                                    MaxOcclusion = rp.maxOccl ?? 0.7,
                                    IsDoor = rp.isDoor ?? false,
                                    IsGlass = rp.isGlass ?? false,
                                };
                                portalInfo.PortalEntityList.Add(portalEntity);
                            });
                        }

                        portalInfoList.Add(portalInfo);
                        portalRoomIdx++;
                    });
            }
        }

        // portalInfoList should be sorted as following order
        // InteriorProxyHash
        // RoomIdx
        // PortalIdx
        // InteriorProxyHash == DestInteriorHash first than sort DestInteriorHash by size

        portalInfoList.Sort((a, b) =>
        {
            var aHash = a.InteriorProxyHash;
            var bHash = b.InteriorProxyHash;
            if (aHash == bHash)
            {
                var aRoomIdx = a.RoomIdx;
                var bRoomIdx = b.RoomIdx;
                if (aRoomIdx == bRoomIdx)
                {
                    var aPortalIdx = a.PortalIdx;
                    var bPortalIdx = b.PortalIdx;
                    return aPortalIdx.CompareTo(bPortalIdx);
                }

                return aRoomIdx.CompareTo(bRoomIdx);
            }

            return aHash.CompareTo(bHash);
        });

        return portalInfoList;
    }

    private List<Xml.PathNode> GeneratePathNodeList(List<PortalInfo> portalInfoList)
    {
        var nodes = new List<PathNodes.PathNode>();
        var roomHashes = new Dictionary<int, int>();
        foreach (var room in mloArchetype.rooms)
        {
            var joaat = new JenkHash(room.Name, JenkHashInputEncoding.UTF8).HashInt;
            var hash = Int32Round(room.Name == "limbo" ? joaat : occlusionHash ^ joaat);
            roomHashes.Add(room.Index, hash);
        }

        List<IndexValue<PortalInfo>> indexedPortalInfoList =
            portalInfoList.Select((p, i) => new IndexValue<PortalInfo>(i, p)).ToList();

        List<NodeGeneration> generationQueue = new List<NodeGeneration>();

        foreach (var (fromRoom, toRooms) in Entry.Paths)
        {
            foreach (var toRoom in toRooms)
            {
                // Find all portals with these rooms
                var portals = indexedPortalInfoList.FindAll(p =>
                    p.Value.RoomIdx == fromRoom && p.Value.DestRoomIdx == toRoom
                );
                var key = Int32Round(roomHashes[fromRoom] - roomHashes[toRoom]);
                if (portals.Count == 0)
                {
                    generationQueue.Add(new NodeGeneration()
                    {
                        PathListKey = key,
                        toRoom = toRoom,
                        FromRoom = fromRoom,
                        hopCount = 0,
                        usedRoom = new List<int>(),
                        path = new List<List<NodeGenerationEntry>>()
                    });
                }
                else
                {
                    var pathNode = new PathNodes.PathNode()
                    {
                        Key = key,
                        EntList = portals.ConvertAll(p => new PathNodes.PathNodeChild()
                        {
                            Key = 0,
                            PortalInfoIdx = p.Index
                        })
                    };
                    nodes.Add(pathNode);
                }
            }
        }

        // TODO: traverse queue and do fun stuff
        // RoomGraph graph = new RoomGraph(portalInfoList);

        bool addedPathNode = true;
        while (generationQueue.Any() && addedPathNode)
        {
            addedPathNode = false;
            foreach (var queue in generationQueue)
            {
                var path = new List<int> {queue.FromRoom};
                var success = queue.SearchPath(ref path, ref indexedPortalInfoList);
                if (success)
                {
                    addedPathNode = true;
                    break;
                }
            }
        }


        return nodes.ConvertAll(n => new Xml.PathNode()
            {
                Key = CreateValue(n.Key.ToString()),
                PathNodeChildList = new PathNodeChildList()
                {
                    PortalEntList = n.EntList.ConvertAll(ent => new Xml.PathNodeChild()
                    {
                        PathNodeKey = CreateValue(ent.Key.ToString()),
                        PortalInfoIdx = CreateValue(ent.PortalInfoIdx.ToString())
                    })
                }
            }
        );
    }

    // TODO: Move to a better place
    private void GenerateOcclusionMetadata()
    {
        var metadata = new naOcclusionInteriorMetadata();
        var portalInfoList = GeneratePortalInfoList();
        metadata.PortalInfoList.PortalInfoList = portalInfoList.ConvertAll(Xml.PortalInfo.FromMetadataPortal);
        metadata.PathNodeList.PortalInfoList = GeneratePathNodeList(portalInfoList);

        var xmlFilePath = Path.Join(outputPath, $"{occlusionHash}.ymt.pso.xml");
        Util.Xml.WriteXml(xmlFilePath, metadata);

        // var ymtPath = Path.Join(outputPath, $"{occlusionHash}.ymt");
        // XmlDocument dat54Doc = new();
        // dat54Doc.Load(xmlFilePath);
        // var ymtFile = XmlRel.GetRel(dat54Doc);
        // File.WriteAllBytes(ymtPath, ymtFile.Save());
    }

    private void LoadYmap()
    {
        var ymapPath = Path.Join(_model.data.assetsPath, Entry.ymapPath);
        if (!File.Exists(ymapPath))
        {
            throw new DataException($"Ymap file does not exist.{Entry.name}");
        }

        var YmapBytes = File.ReadAllBytes(ymapPath);
        ymap = new YmapFile();
        ymap.Load(YmapBytes);
    }

    private void LoadYtyp()
    {
        var ytypPath = Path.Join(_model.data.assetsPath, Entry.ytypPath);
        if (!File.Exists(ytypPath))
        {
            throw new DataException($"Ytyp file does not exist.{Entry.name}");
        }

        var ytypBytes = File.ReadAllBytes(ytypPath);
        ytyp = new YtypFile();
        ytyp.Load(ytypBytes);
        mloArchetype = ytyp.AllArchetypes.First(a => a.Type == MetaName.CMloArchetypeDef) as MloArchetype;
    }

    private void CalculateOcclusionHash()
    {
        var mloEnt = ymap.MloEntities.First();
        var intArchHash = Convert.ToInt32(mloEnt.EntityHash);
        occlusionHash = Int32Round(intArchHash ^ Convert.ToInt32(mloEnt.Position.X * 100) ^
                                   Convert.ToInt32(mloEnt.Position.Y * 100) ^
                                   Convert.ToInt32(mloEnt.Position.Z * 100));
        Debug.WriteLine($"Occlusion hash: {occlusionHash}");
    }

    public void Start()
    {
        LoadYmap();
        LoadYtyp();
        CalculateOcclusionHash();
        // Select output folder
        var dialog = new FolderBrowserDialog()
        {
            InitialDirectory = outputPath != "" ? outputPath : _model.data.assetsPath,
            Description = "Select the output folder for the occlusion files.",
            ShowNewFolderButton = true,
            UseDescriptionForTitle = true
        };
        var dialogResult = dialog.ShowDialog();
        if (dialogResult != DialogResult.OK)
        {
            MessageBox.Show("No output folder selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly, false);
            return;
        }

        outputPath = dialog.SelectedPath;

        GenerateOcclusionMetadata();
    }
}