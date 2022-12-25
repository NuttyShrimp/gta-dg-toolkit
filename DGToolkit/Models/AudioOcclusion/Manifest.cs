using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using Newtonsoft.Json;

namespace DGToolkit.Models.AudioOcclusion;

public class InteriorRoom
{
    public string? roomIndex { get; set; }
    public float? reverb { get; set; }
    public float? echo { get; set; }
}

public class InteriorPortal
{
    public int? portalId { get; set; }
    public int? srcRoomIdx { get; set; }
    public int? destRoomIdx { get; set; }
    public float? maxOccl { get; set; }
    public bool? isDoor { get; set; }
    public bool? isGlass { get; set; }
}

public class InteriorEntry
{
    // Relative to assetsPath
    public int? index { get; set; }
    public string? name { get; set; }
    public string? ymapPath { get; set; }
    public string? ytypPath { get; set; }
    public Dictionary<int, int>? paths { get; set; }
    public List<InteriorRoom> rooms { get; set; }
    public List<InteriorPortal> portals { get; set; }

    public bool ShouldSerializeindex()
    {
        return false;
    }

    public InteriorEntry deepCopy()
    {
        var entry = (InteriorEntry) MemberwiseClone();
        if (entry.paths != null)
        {
            entry.paths = new Dictionary<int, int>(paths);
        }

        entry.rooms = new List<InteriorRoom>(rooms);
        entry.portals = new List<InteriorPortal>(portals);
        return entry;
    }
}

public class Manifest
{
    // Path to [assets] folder in resources folder
    public string? assetsPath { get; set; }
    public List<InteriorEntry> interiors { get; set; }
}

public class Parser
{
    private readonly string dataDirPath = Path.Combine(AppContext.BaseDirectory, "../../../Data");

    private void ValidateDataPath()
    {
        if (!Directory.Exists(dataDirPath))
        {
            MessageBox.Show($"Could not find the data directory at {dataDirPath}",
                "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
            Application.Current.Shutdown();
        }

        string manifestPath = Path.Combine(dataDirPath, "./audiomanifest.json");

        if (!File.Exists(manifestPath))
        {
            MessageBox.Show($"Could not find the audio occlusion manifest file at {manifestPath}",
                "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
            Application.Current.Shutdown();
        }
    }

    public Manifest ImportManifest()
    {
        ValidateDataPath();
        string manifestPath = Path.Combine(dataDirPath, "./occlmanifest.json");
        Manifest? manifest = JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(manifestPath));
        if (manifest == null)
        {
            MessageBox.Show($"The occlusion manifest file at {manifestPath} is empty or configured badly",
                "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
            Application.Current.Shutdown();
        }

        int idx = 0;
        manifest.interiors = manifest.interiors.ConvertAll(i =>
        {
            i.index = idx++;
            return i;
        });

        return manifest;
    }

    public void SaveManifest(Manifest data)
    {
        ValidateDataPath();
        string manifestPath = Path.Combine(dataDirPath, "./occlmanifest.json");
        var jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(manifestPath, jsonString);
    }
}