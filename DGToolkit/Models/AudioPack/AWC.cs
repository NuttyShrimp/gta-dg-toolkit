using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Documents;
using System.Xml.Serialization;
using DGToolkit.Models.Util;
using static DGToolkit.Models.Util.Util;

namespace DGToolkit.Models.AudioPack.AWC;

public class File
{
    public class Unk
    {
        [XmlAttribute] public string unk { get; set; }
    }

    public class ChunkItem
    {
        public string? Type { get; set; }
        public string? Codec { get; set; }
        public Value? Samples { get; set; }
        public Value? SampleRate { get; set; }
        public Value? Headroom { get; set; }
        public Value? PlayBegin { get; set; }
        public Value? PlayEnd { get; set; }
        public Value? LoopBegin { get; set; }
        public Value? LoopEnd { get; set; }
        public Value? LoopPoint { get; set; }
        public Value? BlockSize { get; set; }
        public Unk? Peak { get; set; }
    }

    public class StreamItem
    {
        public string? Name { get; set; }
        public string? FileName { get; set; }

        [XmlArrayItem("Item")] public ChunkItem[] Chunks { get; set; }
    }

    public class AudioWaveContainer
    {
        public Value ChunkIndices = new()
        {
            value = "True"
        };

        [XmlArrayItem("Item")] public StreamItem[] Streams = Array.Empty<StreamItem>();

        public Value Version = new()
        {
            value = "1"
        };
    }
}

public class Generator
{
    private static File.ChunkItem[] GenerateChunks(double samples, double samplerate)
    {
        return new[]
        {
            new File.ChunkItem() {Type = "peak"},
            new File.ChunkItem {Type = "data"},
            new File.ChunkItem
            {
                Type = "format",
                Codec = "ADPCM",
                Samples = CreateValue($"{samples}"),
                SampleRate = CreateValue($"{samplerate}"),
                Headroom = CreateValue("161"),
                PlayBegin = CreateValue("0"),
                PlayEnd = CreateValue("0"),
                LoopBegin = CreateValue("0"),
                LoopEnd = CreateValue("0"),
                LoopPoint = CreateValue("-1"),
                Peak = new File.Unk {unk = "0"}
            }
        };
    }

    public static void GenerateAWCXml(string path, List<FileEntry> files)
    {
        var awc = new File.AudioWaveContainer();
        awc.Version = CreateValue("1");
        awc.ChunkIndices = CreateValue("True");
        List<File.StreamItem> _streams = new();
        foreach (var file in files)
        {
            // add left & right stream
            var side = "_l";
            // loop 2 times
            for (var i = 0; i < 2; i++)
            {
                File.StreamItem item = new();
                var formattedStr = Util.Util.CustomTrimmer(Path.GetFileNameWithoutExtension(file.name));
                item.Name = $"{formattedStr}{side}";
                item.FileName = $"{formattedStr}{side}.wav";
                item.Chunks = GenerateChunks(file.samples, file.sampleRate);
                _streams.Add(item);
                side = "_r";
            }
        }

        awc.Streams = _streams.ToArray();

        Xml.WriteXml(path, awc);
    }
}