using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using Newtonsoft.Json;
using File = System.IO.File;

namespace DGToolkit.Models.AudioPack
{
    public class FileEntry
    {
        public string name { get; set; }
        public bool looped { get; set; }
        public Dat54.BaseSoundHeader headers { get; set; }
        [JsonIgnore]
        public double samples { get; set; }
        [JsonIgnore]
        public double sampleRate { get; set; }
    }

    // Corresponds with a dlc_{dlcName}/{DLCEntry.name} in the output
    public class DLCEntry
    {
        public string name { get; set; }
        public List<FileEntry> files { get; set; }
    }

    public class Manifest
    {
        public string dlcName { get; set; }
        public string outputPath { get; set; }
        public string dataPath { get; set; }
        public List<DLCEntry> data { get; set; }
        public ObservableCollection<string> packNames { get; set; }

        public DLCEntry GetPackInfo(string key)
        {
            return data.First(e => e.name == key);
        }

        public bool ShouldSerializedataPath()
        {
            return false;
        }
    }


    public class Parser
    {
        private readonly string dataDirPath = Path.Combine(AppContext.BaseDirectory, "../../../Data");

        private void ValidateDataPath(string path)
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

        public Manifest ImportManifest()
        {
            // We know that the program shutdown if there was an error
            ValidateDataPath(dataDirPath);

            string manifestPath = Path.Combine(dataDirPath, "./audiomanifest.json");
            Manifest? manifest = JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(manifestPath));
            if (manifest == null)
            {
                MessageBox.Show($"The audiomanifest file at {manifestPath} is empty or configured badly",
                    "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }

            manifest!.dataPath = dataDirPath;
            manifest!.packNames = new ObservableCollection<string>();
            foreach (var dlcEntry in manifest!.data)
            {
                manifest!.packNames.Add(dlcEntry.name);
            }

            return manifest;
        }

        // Write the given manifest to its data location
        // returns a bool indicating if the write process was successful
        public bool WriteManifest(Manifest manifest)
        {
            // Check if audioManifest.json is available in our manifest.dataPath location
            ValidateDataPath(manifest.dataPath);

            var manifestPath = Path.Combine(manifest.dataPath, "./audiomanifest.json");
            var jsonString = JsonConvert.SerializeObject(manifest, Formatting.Indented);
            File.WriteAllText(manifestPath, jsonString);
            return true;
        }
    }
}