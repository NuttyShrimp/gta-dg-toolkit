using System;
using System.Collections.Generic;

namespace DGToolkit.Models.AudioOcclusion;

public class OcclusionModel
{
    private static readonly Lazy<OcclusionModel> lazy = new Lazy<OcclusionModel>(() => new OcclusionModel());
    public static OcclusionModel instance => lazy.Value;

    private Parser occlParser;
    public Manifest data;
    public int? selected;
    private InteriorEntry? _copy;

    private OcclusionModel()
    {
        occlParser = new Parser();
        data = occlParser.ImportManifest();
    }

    public void selectEntry(InteriorEntry entry)
    {
        selected = entry.index;
        _copy = entry.deepCopy();
    }

    public void createEntry()
    {
        var entry = new InteriorEntry
        {
            name = $"Interior {data.interiors.Count}",
            ymapPath = "",
            ytypPath = "",
            paths = new Dictionary<int, int>(),
            portals = new List<InteriorPortal>(),
            rooms = new List<InteriorRoom>(),
        };
        data.interiors.Add(entry);
        this.selectEntry(entry);
    }

    public void reset()
    {
        if (selected == null) return;
        data.interiors[selected.Value] = _copy.deepCopy();
    }

    public void save()
    {
        occlParser.SaveManifest(data);
    }
}