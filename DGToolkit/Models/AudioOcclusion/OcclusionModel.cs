using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CodeWalker.GameFiles;
using Xabe.FFmpeg;

namespace DGToolkit.Models.AudioOcclusion;

public class OcclusionModel
{
    private static readonly Lazy<OcclusionModel> lazy = new Lazy<OcclusionModel>(() => new OcclusionModel());
    public static OcclusionModel instance => lazy.Value;

    private Parser occlParser;
    public Manifest data;
    public Prop<int?> selected;
    private InteriorEntry? _copy;

    private OcclusionModel()
    {
        occlParser = new Parser();
        data = occlParser.ImportManifest();
        selected = new Prop<int?>();
    }

    private string generatePath(string path)
    {
        return Path.Join(data.assetsPath, path);
    }

    private MloArchetype? getArchetype(ref InteriorEntry entry)
    {
        var ytypPath = generatePath(entry.ytypPath);
        if (entry.ytypPath == "" || !File.Exists(ytypPath)) return null;
        // Load entry.ytypPath
        var ytypBytes = File.ReadAllBytes(ytypPath);
        var ytypFile = new YtypFile();
        ytypFile.Load(ytypBytes);
        return ytypFile.AllArchetypes.First(a => a.Type == MetaName.CMloArchetypeDef) as MloArchetype;
    }

    private Dictionary<int, HashSet<int>> LoadPaths(InteriorEntry entry)
    {
        var paths = new Dictionary<int, HashSet<int>>();
        var roomArchetype = getArchetype(ref entry);
        if (roomArchetype == null)
        {
            return paths;
        }

        for (int i = 0; i < roomArchetype.rooms.Length; i++)
        {
            paths.Add(i, new HashSet<int>());
        }

        foreach (var portal in roomArchetype.portals)
        {
            paths[(int) portal.Data.roomFrom].Add((int) portal.Data.roomTo);
            paths[(int) portal.Data.roomTo].Add((int) portal.Data.roomFrom);
        }

        return paths;
    }

    private ObservableCollection<InteriorRoom> LoadRooms(InteriorEntry entry)
    {
        var rooms = new ObservableCollection<InteriorRoom>();
        var roomArchetype = getArchetype(ref entry);
        if (roomArchetype == null)
        {
            return rooms;
        }

        foreach (var room in roomArchetype.rooms)
        {
            rooms.Add(new InteriorRoom()
            {
                name = room.Name,
                roomIndex = room.Index,
                echo = 0,
                reverb = 0,
            });
        }

        return rooms;
    }

    private ObservableCollection<InteriorPortal> LoadPortals(InteriorEntry entry)
    {
        var portals = new ObservableCollection<InteriorPortal>();
        var roomArchetype = getArchetype(ref entry);
        if (roomArchetype == null)
        {
            return portals;
        }

        var portalRoomIdxs = new Dictionary<int, int>();
        foreach (var portal in roomArchetype.portals)
        {
            if (portal.AttachedObjects == null || portal.AttachedObjects.Length == 0) continue;

            if (!portalRoomIdxs.ContainsKey((int) portal.Data.roomFrom))
            {
                portalRoomIdxs.Add((int) portal.Data.roomFrom, 0);
            }
            else
            {
                portalRoomIdxs[(int) portal.Data.roomFrom]++;
            }

            if (!portalRoomIdxs.ContainsKey((int) portal.Data.roomTo))
            {
                portalRoomIdxs.Add((int) portal.Data.roomTo, 0);
            }
            else
            {
                portalRoomIdxs[(int) portal.Data.roomTo]++;
            }

            for (var i = 0; i < portal.AttachedObjects.Length; i++)
            {
                var entityName = roomArchetype.entities[portal.AttachedObjects[i]].Name.ToLower();
                var entHash = new JenkHash(entityName, JenkHashInputEncoding.UTF8).HashInt;
                portals.Add(new InteriorPortal()
                {
                    entityIdx = i,
                    portalRoomIdx = portalRoomIdxs[(int) portal.Data.roomFrom],
                    entityHash = entHash,
                    portalId = portal.Index,
                    srcRoomIdx = (int) portal.Data.roomFrom,
                    destRoomIdx = (int) portal.Data.roomTo,
                    maxOccl = 0.7,
                    isDoor = true,
                    isGlass = false,
                });
                portals.Add(new InteriorPortal()
                {
                    entityIdx = i,
                    portalRoomIdx = portalRoomIdxs[(int) portal.Data.roomTo],
                    entityHash = entHash,
                    portalId = portal.Index,
                    srcRoomIdx = (int) portal.Data.roomTo,
                    destRoomIdx = (int) portal.Data.roomFrom,
                    maxOccl = 0.7,
                    isDoor = true,
                    isGlass = false,
                });
            }
        }

        
        portals = new ObservableCollection<InteriorPortal>(portals.OrderBy(p => p.portalId).ThenBy(p => p.srcRoomIdx)
            .ThenBy(p => p.entityIdx));
        return portals;
    }

    public void entryUpdate()
    {
        if (selected.Value == null) return;
        if (_copy.ytypPath != data.interiors[selected.Value.Value].ytypPath)
        {
            data.interiors[selected.Value.Value].paths = LoadPaths(data.interiors[selected.Value.Value]);
            data.interiors[selected.Value.Value].rooms = LoadRooms(data.interiors[selected.Value.Value]);
            data.interiors[selected.Value.Value].portals = LoadPortals(data.interiors[selected.Value.Value]);
        }
    }

    public void selectEntry(InteriorEntry entry)
    {
        if (entry.index == null) return;
        selected.Value = entry.index;
        if (entry.paths.Count == 0)
        {
            entry.paths = LoadPaths(entry);
        }

        if (entry.rooms.Count == 0)
        {
            entry.rooms = LoadRooms(entry);
        }

        if (entry.portals.Count == 0)
        {
            entry.portals = LoadPortals(entry);
        }

        _copy = entry.deepCopy();
    }

    public void createEntry()
    {
        var entry = new InteriorEntry
        {
            name = $"Interior {data.interiors.Count}",
            ymapPath = "",
            ytypPath = "",
            paths = new Dictionary<int, HashSet<int>>(),
            portals = new ObservableCollection<InteriorPortal>(),
            rooms = new ObservableCollection<InteriorRoom>(),
            index = data.interiors.Count
        };
        data.interiors.Add(entry);
        this.selectEntry(entry);
    }

    public void reset()
    {
        if (selected.Value == null) return;
        data.interiors[selected.Value.Value] = _copy.deepCopy();
    }

    public void save()
    {
        occlParser.SaveManifest(data);
    }
}