using System;
using System.IO;
using System.Windows;
using DGToolkit.Models.AudioPack.Dat54;
using Newtonsoft.Json;

namespace DGToolkit.Models.AudioPack
{
    public class FileEntry
    {
        public string name { get; set; }
        public bool looped { get; set; }
        public baseSoundHeader headers { get; set; }
        public double samples { get; set; }
        public double sampleRate { get; set; }
    }

    // Corresponds with a dlc_{dlcName}/{DLCEntry.name} in the output
    public class DLCEntry
    {
        public string name { get; set; }
        public FileEntry[] files { get; set; }
    }

    public class Manifest
    {
        public string dlcName { get; set; }
        public string outputPath { get; set; }
        public string dataPath { get; set; }
        public DLCEntry[] data { get; set; }
    }


    public class Parser
    {
        private readonly string path = Path.Combine(AppContext.BaseDirectory, "../../Data");

        public Manifest ImportManifest()
        {
            if (!Directory.Exists(path))
            {
                MessageBox.Show($"Could not find the data directory at {path}",
                    "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Windows.Application.Current.Shutdown();
            }

            string manifestPath = Path.Combine(path, "./audiomanifest.json");

            if (!File.Exists(manifestPath))
            {
                MessageBox.Show($"Could not find the audiomanifest file at {manifestPath}",
                    "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Windows.Application.Current.Shutdown();
            }

            Manifest? manifest = JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(manifestPath));
            if (manifest == null)
            {
                MessageBox.Show($"The audiomanifest file at {manifestPath} is empty or configured badly",
                    "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Windows.Application.Current.Shutdown();
            }

            manifest!.dataPath = path;
            return manifest;
        }
    }
}