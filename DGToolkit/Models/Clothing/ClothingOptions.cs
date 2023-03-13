using System.IO;
using System.Windows;
using Newtonsoft.Json;

namespace DGToolkit.Models.Clothing;

public class ClothingOptions
{
    public Options.File? data;

    public ClothingOptions()
    {
        this.LoadOptions();
    }

    private void LoadOptions()
    {
        // We know that the program shutdown if there was an error
        Util.DataDir.ValidateDataPath(Util.DataDir.dataDirPath);

        string dataPath = Path.Combine(Util.DataDir.dataDirPath, "./clothingdata.json");
        data = JsonConvert.DeserializeObject<Options.File>(File.ReadAllText(dataPath));
        if (data == null)
        {
            MessageBox.Show($"The clothing data file at {dataPath} is empty or configured badly",
                "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
            Application.Current.Shutdown();
        }
    }
}