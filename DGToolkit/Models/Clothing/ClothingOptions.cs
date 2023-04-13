using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using DGToolkit.Models.Clothing.Options;
using Newtonsoft.Json;

namespace DGToolkit.Models.Clothing;

public class ClothingOptions
{
    public Options.ClothingManifest? data;
    public ClothingStore store;

    public ClothingOptions(ClothingStore store)
    {
        this.store = store;
        LoadOptions();
    }

    private void LoadOptions()
    {
        // We know that the program shutdown if there was an error
        Util.DataDir.ValidateDataPath(Util.DataDir.dataDirPath);

        string dataPath = Path.Combine(Util.DataDir.dataDirPath, "./clothingdata.json");
        data = JsonConvert.DeserializeObject<Options.ClothingManifest>(File.ReadAllText(dataPath));
        if (data == null)
        {
            MessageBox.Show($"The clothing data file at {dataPath} is empty or configured badly",
                "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
            Application.Current.Shutdown();
            return;
        }

        data.ResourceFolder = Properties.Settings.Default.ClothingResourceFolder;
    }

    public void SaveOptions()
    {
        // We know that the program shutdown if there was an error
        Util.DataDir.ValidateDataPath(Util.DataDir.dataDirPath);
        Properties.Settings.Default.ClothingResourceFolder = data.ResourceFolder;
        Properties.Settings.Default.Save();

        if (store.selectedDLC != null)
        {
            if (!data.PedEntries.ContainsKey(store.selectedDLC.fullDlcName))
            {
                data.PedEntries.Add(store.selectedDLC.fullDlcName, new List<ClothInfo>());
            }


            var savedInfoList = data.PedEntries[store.selectedDLC.fullDlcName];
            savedInfoList.Clear();
            foreach (var clothData in store.Clothes)
            {
                savedInfoList.Add(new(clothData));
            }
        }

        string dataPath = Path.Combine(Util.DataDir.dataDirPath, "./clothingdata.json");
        string jsonStr = JsonConvert.SerializeObject(data, Formatting.Indented);
        if (jsonStr == null)
        {
            MessageBox.Show($"The clothing data file at {dataPath} is empty or configured badly",
                "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
            Application.Current.Shutdown();
        }

        File.WriteAllText(dataPath, jsonStr);
        store.unsavedChanges = false;
    }
}