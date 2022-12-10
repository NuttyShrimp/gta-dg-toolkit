using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using CodeWalker.GameFiles;
using DGToolkit.Models.Util;
using static DGToolkit.Models.Util.Util;


namespace DGToolkit.Models.AudioPack.Dat54;

public class BaseSoundHeader : IEditableObject
{
    public int? volume { get; set; }
    public int? dopplerFactor { get; set; }
    public string? categoryHash { get; set; }
    public string? rolloffHash { get; set; }
    public int? distance { get; set; }
    public int? echox { get; set; }
    public int? echoy { get; set; }
    public int? echoz { get; set; }

    public BaseSoundHeader Copy()
    {
        return (BaseSoundHeader) this.MemberwiseClone();
    }

    public void BeginEdit()
    {
        throw new System.NotImplementedException();
    }

    public void CancelEdit()
    {
        throw new System.NotImplementedException();
    }

    public void EndEdit()
    {
        throw new System.NotImplementedException();
    }
}

public class File
{
    public static class SoundHeaderValues
    {
        public const int Volume = 0x00000004;
        public const int DopplerFactor = 0x00004000;
        public const int Category = 0x00008000;
        public const int RolloffCurve = 0x00100000;
        public const int DistanceAttenuation = 0x00200000;
        public const int Unk20 = 0x00800000;
        public const int Unk22 = 0x08000000;
        public const int Unk23 = 0x10000000;
        public const int Unk24 = 0x20000000;
    }

    public class SoundHeader
    {
        public string? Category;
        public Value? DistanceAttenuation;
        public Value? DopplerFactor;
        public Value? Flags;
        public string? RolloffCurve;
        public Value? Unk20;
        public Value? Unk22;
        public Value? Unk23;
        public Value? Unk24;
        public Value? Volume;
    }

    public class SetItem
    {
        public string? ScriptName { get; set; }
        public string? ChildSound { get; set; }
    }

    public class SoundItem
    {
        [XmlArrayItem("Item")] public string[] ChildSounds;

        [XmlArrayItem("Item")] public SetItem[] Items;

        [XmlAttribute] public string? type { get; set; }

        public string? Name { get; set; }
        public SoundHeader? Header { get; set; }
        public string? ContainerName { get; set; }
        public string? FileName { get; set; }
        public string? ChildSound { get; set; }
        public string? ChildSound1 { get; set; }
        public string? ChildSound2 { get; set; }
        public string? FallBackSound { get; set; }
        public string? ParameterHash0 { get; set; }
        public string? ParameterHash1 { get; set; }
        public string? ParameterHash2 { get; set; }
        public string? ParameterHash3 { get; set; }
        public string? ParameterHash4 { get; set; }
        public string? ParameterHash5 { get; set; }
        public string? LoopCountParameter { get; set; }

        public Value? WaveSlotNum { get; set; }
        public Value? UnkFloat0 { get; set; }
        public Value? UnkFloat1 { get; set; }
        public Value? UnkInt { get; set; }
        public Value? UnkByte { get; set; }
        public Value? LoopCount { get; set; }
        public Value? LoopCountVariance { get; set; }
        public Value? UnkShort2 { get; set; }
    }

    public class Dat54
    {
        [XmlArrayItem("Item")] public string[] ContainerPaths;

        [XmlArrayItem("Item")] public SoundItem[] Items;

        public Value Version = new()
        {
            value = "7126027"
        };
    }
}

public class Generator
{
    private static Value CalcHeaderFlag(File.SoundHeader header)
    {
        var flag = 0x00000000;
        if (header.Category != null) flag += File.SoundHeaderValues.Category;
        if (header.Volume != null) flag += File.SoundHeaderValues.Volume;
        if (header.DopplerFactor != null) flag += File.SoundHeaderValues.DopplerFactor;
        if (header.RolloffCurve != null) flag += File.SoundHeaderValues.RolloffCurve;
        if (header.DistanceAttenuation != null) flag += File.SoundHeaderValues.DistanceAttenuation;
        if (header.Unk20 != null) flag += File.SoundHeaderValues.Unk20;
        if (header.Unk22 != null) flag += File.SoundHeaderValues.Unk22;
        if (header.Unk23 != null) flag += File.SoundHeaderValues.Unk23;
        if (header.Unk24 != null) flag += File.SoundHeaderValues.Unk24;
        return CreateValue($"0x{flag.ToString("X8")}");
    }

    private static File.SoundItem GetSimpleSound(string name, string rpfPath, int volume)
    {
        File.SoundHeader header = new()
        {
            Volume = CreateValue($"{volume}")
        };
        header.Flags = CalcHeaderFlag(header);
        return new File.SoundItem
        {
            type = "SimpleSound",
            Name = name,
            Header = header,
            ContainerName = rpfPath,
            FileName = name,
            WaveSlotNum = CreateValue("0")
        };
    }

    private static File.SoundItem GetStereoSound(string name, int volume)
    {
        File.SoundHeader header = new()
        {
            Volume = CreateValue($"{volume}")
        };
        header.Flags = CalcHeaderFlag(header);
        return new File.SoundItem
        {
            type = "CollapsingStereoSound",
            Name = $"{name}_css",
            Header = header,
            ChildSound1 = $"{name}_l",
            ChildSound2 = $"{name}_r",
            UnkFloat0 = CreateValue("0"),
            UnkFloat1 = CreateValue("0"),
            ParameterHash0 = "",
            ParameterHash1 = "",
            ParameterHash2 = "",
            ParameterHash3 = "",
            ParameterHash4 = "",
            ParameterHash5 = "",
            UnkInt = CreateValue("1065353216"),
            UnkByte = CreateValue("0")
        };
    }

    private static File.SoundItem GetLoopingSound(string name, int volume)
    {
        File.SoundHeader header = new()
        {
            Volume = CreateValue($"{volume}")
        };
        header.Flags = CalcHeaderFlag(header);
        return new File.SoundItem
        {
            type = "LoopingSound",
            Name = $"{name}_loop",
            Header = header,
            LoopCount = CreateValue("-1"),
            LoopCountVariance = CreateValue("0"),
            UnkShort2 = CreateValue("0"),
            ChildSound = $"{name}_css",
            LoopCountParameter = ""
        };
    }

    private static File.SoundItem GetMultitrackSound(string name, BaseSoundHeader entryHeaders, bool isLooped)
    {
        File.SoundHeader header = new()
        {
            Unk20 = CreateValue("0")
        };
        foreach (var property in entryHeaders.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.Name.Equals("volume")) header.Volume = CreateValue($"{property.GetValue(entryHeaders)}");
            if (property.Name.Equals("dopplerFactor"))
                header.DopplerFactor = CreateValue($"{property.GetValue(entryHeaders)}");
            if (property.Name.Equals("distance"))
                header.DistanceAttenuation = CreateValue($"{property.GetValue(entryHeaders)}");
            if (property.Name.Equals("echox")) header.Unk22 = CreateValue($"{property.GetValue(entryHeaders)}");
            if (property.Name.Equals("echoy")) header.Unk23 = CreateValue($"{property.GetValue(entryHeaders)}");
            if (property.Name.Equals("echoz")) header.Unk24 = CreateValue($"{property.GetValue(entryHeaders)}");
            if (property.Name.Equals("categoryHash")) header.Category = $"hash_{property.GetValue(entryHeaders)}";
            if (property.Name.Equals("rolloffHash")) header.RolloffCurve = $"hash_{property.GetValue(entryHeaders)}";
        }

        header.Flags = CalcHeaderFlag(header);
        var childSoundAppendix = isLooped ? "loop" : "css";
        return new File.SoundItem
        {
            type = "MultitrackSound",
            Name = $"{name}_mt",
            Header = header,
            ChildSounds = new[]
            {
                $"{name}_{childSoundAppendix}"
            }
        };
    }

    private static File.SoundItem GetSoundSet(List<FileEntry> files, string rpfName)
    {
        var setItems = files
            .Select(file => CustomTrimmer(Path.GetFileNameWithoutExtension(file.name))).Select(scriptName =>
                new File.SetItem {ScriptName = scriptName, ChildSound = $"{scriptName}_mt"}).ToList();

        var soundSet = new File.SoundItem
        {
            type = "SoundSet",
            Name = rpfName.Replace("/", "_"),
            Header = new File.SoundHeader
            {
                Flags = CreateValue("0xAAAAAAAA")
            },
            Items = setItems.OrderBy(x => new JenkHash(x.ScriptName, JenkHashInputEncoding.UTF8).HashUint).ToArray()
        };
        return soundSet;
    }

    public static void GenerateDat54Xml(string path, string rpfPath, List<FileEntry> files)
    {
        List<File.SoundItem> simpleItems = new();
        List<File.SoundItem> StereoItems = new();
        List<File.SoundItem> LoopingItems = new();
        List<File.SoundItem> MultitrackItems = new();
        // Generate all simple Sounds
        foreach (var file in files)
        {
            var trimmedName = CustomTrimmer(Path.GetFileNameWithoutExtension(file.name));
            simpleItems.Add(GetSimpleSound($"{trimmedName}_l", rpfPath, file.headers.volume ?? 5));
            simpleItems.Add(GetSimpleSound($"{trimmedName}_r", rpfPath, file.headers.volume ?? 5));
            StereoItems.Add(GetStereoSound(trimmedName, file.headers.volume ?? 5));
            if (file.looped) LoopingItems.Add(GetLoopingSound(trimmedName, file.headers.volume ?? 5));
            MultitrackItems.Add(GetMultitrackSound(trimmedName, file.headers, file.looped));
        }

        var items = simpleItems.Concat(StereoItems).Concat(LoopingItems).Concat(MultitrackItems).ToList();
        items.Add(GetSoundSet(files, rpfPath));
        var dat54 = new File.Dat54
        {
            Version = CreateValue("7314721"),
            ContainerPaths = new[] {rpfPath},
            Items = items.ToArray()
        };
        Xml.WriteXml(path, dat54);
    }
}