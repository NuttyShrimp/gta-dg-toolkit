using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using System.Xml.Serialization;
using Xabe.FFmpeg;

namespace DGToolkit.Models.Util;

internal class Log
{
    public static void Error(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(msg);
        Console.ResetColor();
    }

    public static void Warn(string msg)
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine(msg);
        Console.ResetColor();
    }
}

internal class Xml
{
    public static void WriteXml(string path, object container)
    {
        // remove file if exists
        if (File.Exists(path)) File.Delete(path);
        using var stream = File.Open(path, FileMode.OpenOrCreate);

        XmlWriterSettings xmlWriterSettings = new();
        xmlWriterSettings.Indent = true;
        var xmlWriter = XmlWriter.Create(stream, xmlWriterSettings);
        XmlSerializer x = new(container.GetType());
        x.Serialize(xmlWriter, container,
            new XmlSerializerNamespaces(new[] {XmlQualifiedName.Empty})
        );
    }
}

internal class FFMPEG
{
    /// <summary>
    ///   Wrapper for FFmpeg conversion
    /// </summary>
    /// <param name="input">Path to file WITHOUT ext</param>
    /// <param name="output">String to dir where converted files need to go</param>
    public async Task ConvertFile(string input, string output)
    {
        // Strip name from input file
        var fileName = Util.CustomTrimmer(Path.GetFileNameWithoutExtension(input));
        var args =
            $"-i {input.Escape()} -fflags +bitexact -flags:v +bitexact -flags:a +bitexact -acodec \"pcm_s16le\" -ac 1 -filter_complex \"channelsplit = channel_layout = stereo[l][r]\" -map \"[l]\" {output.Escape()}\\{fileName}_l.wav -acodec \"pcm_s16le\" -ac 1 -fflags +bitexact -flags:v +bitexact -flags:a +bitexact -map \"[r]\" {output.Escape()}\\{fileName}_r.wav";
        await FFmpeg.Conversions.New().Start(args);
    }

    public async Task<AudioInfo> GetFilteredInfo(string path)
    {
        var mediaInfo = await FFmpeg.GetMediaInfo(path);
        var sampleRate = mediaInfo.AudioStreams.First().SampleRate;
        return new AudioInfo(Math.Round(mediaInfo.Duration.TotalSeconds * sampleRate, 0), sampleRate);
    }

    internal class AudioInfo
    {
        public AudioInfo(double s, int sr)
        {
            samples = s;
            samplerate = sr;
        }

        public double samples { get; set; }
        public int samplerate { get; set; }
    }
}

public class Util
{
    public static string CustomTrimmer(string input)
    {
        return Regex.Replace(input.ToLower(), @"\s+", "_");
    }

    public static IDictionary<string, string> GetValues(object obj)
    {
        return obj
            .GetType()
            .GetProperties()
            .ToDictionary(p => p.Name, p => p.GetValue(obj).ToString());
    }

    public static Value CreateValue(string val)
    {
        return new Value {value = val};
    }

    public class Value
    {
        [XmlAttribute] public string value { get; set; }
    }

    public static Regex numReg = new Regex("[0-9,.]+");

    public static void NumberInputText(object _, TextCompositionEventArgs e)
    {
        e.Handled = !numReg.IsMatch(e.Text);
    }

    public static String Number2String(int number, bool isCaps)
    {
        Char c = (Char) ((isCaps ? 65 : 97) + (number));
        return c.ToString();
    }

    // 3 length string, padded with 0s in front
    public static String NumberToNumeric(int number)
    {
        return number.ToString().PadLeft(3, '0');
    }
}

public class DataDir
{
    public static readonly string dataDirPath = Path.Combine(AppContext.BaseDirectory, "../../../Data");

    public static void ValidateDataPath(string path)
    {
        if (!Directory.Exists(path))
        {
            MessageBox.Show($"Could not find the data directory at {path}",
                "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
            Application.Current.Shutdown();
        }

        string manifestPath = Path.Combine(path, "./audiomanifest.json");

        if (!File.Exists(manifestPath))
        {
            MessageBox.Show($"Could not find the audiomanifest file at {manifestPath}",
                "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
            Application.Current.Shutdown();
        }
    }
}