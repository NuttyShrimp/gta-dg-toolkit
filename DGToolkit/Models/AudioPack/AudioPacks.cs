using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Xml;
using CodeWalker.GameFiles;
using DGToolkit.Models.Util;
using DGToolkit.Views.AudioPack;
using Xabe.FFmpeg.Downloader;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace DGToolkit.Models.AudioPack;

struct State
{
    public Manifest manifest;
    public ObservableCollection<string> oversized;
}

class AudioPacks
{
    public State state;
    public LogStore logStore;
    private Parser manifestParser;

    public AudioPacks()
    {
        manifestParser = new Parser();
        logStore = new LogStore();
    }

    private void CreateDlcStructure(string entryName)
    {
        var outputInfo = new DirectoryInfo(state.manifest.outputPath);
        if (!outputInfo.Exists) throw new Exception($"Output does not exist path: {state.manifest.outputPath}");

        var dataInfo = new DirectoryInfo(Path.Combine(state.manifest.outputPath, "data"));
        if (!dataInfo.Exists) Directory.CreateDirectory(Path.Combine(outputInfo.FullName, "data"));

        // Check if rel file exists for dlc, if true --> remove
        foreach (var file in dataInfo.GetFiles())
            if (file.Name == $"dlc_{state.manifest.dlcName}_{entryName}")
                file.Delete();

        // Create dlc_xxx_xxx dir for awc file
        var rpfInfo = new DirectoryInfo(Path.Combine(state.manifest.outputPath,
            $"dlc_{state.manifest.dlcName}"));
        // if (rpfInfo.Exists) rpfInfo.Delete(true);

        rpfInfo.Create();
        // Create _tmp folder for generated unwanted files (xml, wav)
        var tmpInfo = new DirectoryInfo(Path.Combine(state.manifest.outputPath, "_tmp"));
        if (!tmpInfo.Exists) tmpInfo.Create();
    }

    private async Task<FFMPEG.AudioInfo> ConvertAudioFile(string inputPath)
    {
        // Check if file exists
        var inputInfo = new FileInfo(inputPath);
        if (!inputInfo.Exists) throw new Exception($"Audio file at {inputPath} could not be found");

        // Create output folder
        var tmpInfo =
            new DirectoryInfo(Path.Combine(state.manifest.outputPath, "_tmp", inputInfo.Directory.Name));
        if (!tmpInfo.Exists) tmpInfo.Create();

        // Check if files with same name exists
        foreach (var file in tmpInfo.GetFiles())
        {
            var fileName = Util.Util.CustomTrimmer(Path.GetFileNameWithoutExtension(inputPath));
            if (file.Name.Equals($"{fileName}_l.wav") || file.Name.Equals($"{fileName}_r.wav")) file.Delete();
        }

        var ffmpeg = new FFMPEG();

        await ffmpeg.ConvertFile(Path.Combine(inputInfo.DirectoryName, Path.GetFileName(inputPath)),
            tmpInfo.FullName);
        return await ffmpeg.GetFilteredInfo(Path.Combine(tmpInfo.FullName,
            $"{Util.Util.CustomTrimmer(Path.GetFileNameWithoutExtension(inputPath))}_l.wav"));
    }


    private async Task<bool> GenerateAwcFile(DLCEntry entry, Action<string> addLog)
    {
        // Entry.name --> dlc_dlcName_entryName
        CreateDlcStructure(entry.name);
        // Convert audio to wav
        // TODO: Implement a TaskScheduler
        foreach (var file in entry.files)
        {
            addLog($"Starting audio conversion of {file.name}");
            var audioInfo = await ConvertAudioFile(Path.Combine(state.manifest.dataPath, entry.name, file.name));
            addLog($"Finished audio conversion of {file.name}");
            file.samples = audioInfo.samples;
            file.sampleRate = audioInfo.samplerate;
        }

        var trimmedName = Util.Util.CustomTrimmer(entry.name);
        // Generate XML file for AWC
        var awcXmlPath =
            Path.Combine(state.manifest.outputPath, "_tmp", $"dlc_{state.manifest.dlcName}_{trimmedName}.awc.xml");
        AWC.Generator.GenerateAWCXml(awcXmlPath, entry.files);
        // Generate AWC file from XML
        var awcPath = Path.Combine(state.manifest.outputPath, $"dlc_{state.manifest.dlcName}",
            $"{trimmedName}.awc");
        XmlDocument awcDoc = new();
        awcDoc.Load(awcXmlPath);
        var awcFile = XmlAwc.GetAwc(awcDoc, Path.Combine(state.manifest.outputPath, "_tmp", entry.name));
        byte[] awcBytes = awcFile.Save();
        File.WriteAllBytes(awcPath, awcBytes);
        return awcBytes.Length > 2097152;
    }

    private void GenerateDatFile(DLCEntry entry)
    {
        var trimmedName = Util.Util.CustomTrimmer(entry.name);
        var datXmlPath = Path.Combine(state.manifest.outputPath, "_tmp",
            $"dlc_{state.manifest.dlcName}_{trimmedName}.dat54.rel.xml");
        Dat54.Generator.GenerateDat54Xml(datXmlPath, $"dlc_{state.manifest.dlcName}/{trimmedName}", entry.files);
        var datPath = Path.Combine(state.manifest.outputPath, "data", $"{trimmedName}.dat54.rel");
        XmlDocument dat54Doc = new();
        dat54Doc.Load(datXmlPath);
        var rel54File = XmlRel.GetRel(dat54Doc);
        File.WriteAllBytes(datPath, rel54File.Save());
    }

    private void GenerateNameTable(DLCEntry entry)
    {
        // This will create a .nametable file in the dat54 dir
        string nameTablePath = Path.Combine(state.manifest.outputPath, "data",
            $"{Util.Util.CustomTrimmer(entry.name)}.dat54.nametable");
        if (File.Exists(nameTablePath))
        {
            File.Delete(nameTablePath);
        }

        Stream content = File.Open(nameTablePath, FileMode.Create);
        BinaryWriter writer = new BinaryWriter(content);

        foreach (var fileEntry in entry.files)
        {
            foreach (var suffix in new string[] {"_l", "_r", "_css", "_loop", "_mt"})
            {
                string name = Util.Util.CustomTrimmer(Path.GetFileNameWithoutExtension(fileEntry.name)) + suffix +
                              char.MinValue;
                byte[] buffer = Encoding.ASCII.GetBytes(name);
                writer.Write(buffer);
            }
        }

        writer.Close();
        content.Close();
    }

    public void Load()
    {
        //Load info from manifest
        state = new State()
        {
            manifest = manifestParser.ImportManifest(),
            oversized = new ObservableCollection<string>()
        };
    }

    public void Save()
    {
        manifestParser.WriteManifest(state.manifest);
    }

    public async Task GeneratePacks()
    {
        using var folderDialog = new FolderBrowserDialog
        {
            Description = "Select an output folder",
            UseDescriptionForTitle = true,
            SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
                           + Path.DirectorySeparatorChar,
            ShowNewFolderButton = true
        };

        if (folderDialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }
        
        state.manifest.outputPath = folderDialog.SelectedPath;
        logStore.AddLogEntry("Setting output path to " + state.manifest.outputPath);

        logStore.AddLogEntry("Downloading ffmpeg");
        await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official);
        logStore.AddLogEntry("Finished downloading ffmpeg");

        foreach (var entry in state.manifest.data)
        {
            try
            {
                logStore.AddLogEntry($"Generating DLC pack: {entry.name}");
                logStore.AddLogEntry("Started AWC generation");
                bool oversized = await GenerateAwcFile(entry, logStore.AddLogEntry);
                if (oversized)
                {
                    state.oversized.Add(entry.name);
                }

                logStore.AddLogEntry("Started Dat54 generation");
                GenerateDatFile(entry);
                logStore.AddLogEntry("Started nametable generation");
                GenerateNameTable(entry);
            }
            catch (Exception e)
            {
                MessageBox.Show("Error doing pack generation: " + e.Message, "Audio pack generation failure",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        logStore.AddLogEntry("Finished pack generation");
    }

    public void AddPack(string name)
    {
        state.manifest.packNames.Add(name);
        state.manifest.data.Add(new DLCEntry()
        {
            name = name,
            files = new List<FileEntry>(),
        });
    }
}