using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using DGToolkit.Models.Clothing;
using DGToolkit.Models.Clothing.Options;
using DGToolkit.Models.Clothing.YMT;
using DGToolkit.Models.Util;
using DGToolkit.Views.AudioPack;
using DataFormats = System.Windows.Forms.DataFormats;
using MessageBox = System.Windows.MessageBox;

namespace DGToolkit.Views.Clothing;

public partial class ClothingView : Page
{
    public ClothingStore store { get; set; }

    public ClothingView()
    {
        store = ClothingStore.Instance;

        InitializeComponent();
        if (Properties.Settings.Default.ClothingResourceFolder == string.Empty)
        {
            SelectResourceFolder();
        }

        AllowDrop = true;
        Drop += HandleFileDrop;
        DLCPackName.DataContext = store.availableDLCS;
        DLCPackName.SelectionChanged += onDLCSelectionChanged;
        DrawableList.ItemsSource = store.Clothes;
        DrawableList.SelectionChanged += (sender, args) =>
        {
            if (DrawableList.SelectedItem == null)
            {
                store.selectedCloth = null;
                DrawableInfoPanel.Visibility = Visibility.Hidden;
                return;
            }

            if (DrawableList.SelectedItem == null) return;
            var item = (ClothData) DrawableList.SelectedItem;
            store.selectedCloth = item;
            TextureList.ItemsSource = item.Textures;
            DrawableInfoPanel.DataContext = item;
            ClothTypeBox.SelectedValue = item.ClothType;
            DrawableInfoPanel.Visibility = Visibility.Visible;
        };
        DrawableInfoPanel.IsVisibleChanged += (sender, args) =>
        {
            if (DrawableInfoPanel.Visibility != Visibility.Visible) return;
            DrawablePosition.DataContext = store.selectedCloth;
            var clothTypeOptions = Types.ClothTypeDescriptions
                .Select(p => new ComboBoxItem() {Content = p.Value, Tag = p.Key}).ToList();
            ClothTypeBox.ItemsSource = clothTypeOptions;
            var selectedItem =
                clothTypeOptions.FindIndex(p => store.selectedCloth.ClothType == (Types.ClothTypes) p.Tag);
            ClothTypeBox.SelectedIndex = selectedItem;
        };
        ClothTypeBox.SelectionChanged += (sender, args) =>
        {
            if (store.selectedCloth == null || ClothTypeBox.SelectedItem == null) return;

            var item = (ComboBoxItem) ClothTypeBox.SelectedItem;
            store.selectedCloth.ClothType = (Types.ClothTypes) item.Tag;

            var drawTypeOptions = Types.ClothDrawableTypes[store.selectedCloth.ClothType]
                .Select(p => new ComboBoxItem()
                    {Content = $"{p} [{ClothNameResolver.DrawableTypeToString(p)}]", Tag = p})
                .ToList();

            DrawTypeBox.ItemsSource = drawTypeOptions;
            DrawTypeBox.SelectedIndex =
                drawTypeOptions.FindIndex(p => store.selectedCloth.DrawableType == (Types.DrawableTypes) p.Tag);
        };
        DrawTypeBox.SelectionChanged += (sender, args) =>
        {
            if (store.selectedCloth == null || DrawTypeBox.SelectedItem == null) return;
            var item = (ComboBoxItem) DrawTypeBox.SelectedItem;
            store.selectedCloth.DrawableType = (Types.DrawableTypes) item.Tag;
        };
    }

    private void onDLCSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DLCPackName.SelectedItem == null)
        {
            DrawableInfoPanel.Visibility = Visibility.Hidden;
            return;
        }

        store.SelectDLC((ShopPedApparel) DLCPackName.SelectedItem);
    }

    private void HandleFileDrop(object sender, System.Windows.DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
        {
            var fileNames = ((string[]) e.Data.GetData(DataFormats.FileDrop)).ToList();
            fileNames.Sort();
            store.addMixedFiles(fileNames);
        }
        // Surely no-one is gonna drop 1 file
        // else if (e.Data.GetDataPresent("FileGroupDescriptor"))
        // {
        //     //
        //     // the first step here is to get the filename
        //     // of the attachment and
        //     // build a full-path name so we can store it
        //     // in the temporary folder
        //     //
        //
        //     // set up to obtain the FileGroupDescriptor
        //     // and extract the file name
        //     Stream theStream = (Stream) e.Data.GetData("FileGroupDescriptor");
        //     byte[] fileGroupDescriptor = new byte[512];
        //     theStream.Read(fileGroupDescriptor, 0, 512);
        //     // used to build the filename from the FileGroupDescriptor block
        //     StringBuilder fileName = new StringBuilder("");
        //     // this trick gets the filename of the passed attached file
        //     for (int i = 76; fileGroupDescriptor[i] != 0; i++)
        //     {
        //         fileName.Append(Convert.ToChar(fileGroupDescriptor[i]));
        //     }
        //
        //     theStream.Close();
        //     string path = Path.GetTempPath();
        //     // put the zip file into the temp directory
        //     string theFile = path + fileName.ToString();
        //     // create the full-path name
        //
        //     //
        //     // Second step:  we have the file name.
        //     // Now we need to get the actual raw
        //     // data for the attached file and copy it to disk so we work on it.
        //     //
        //
        //     // get the actual raw file into memory
        //     MemoryStream ms = (MemoryStream) e.Data.GetData(
        //         "FileContents", true);
        //     // allocate enough bytes to hold the raw data
        //     byte[] fileBytes = new byte[ms.Length];
        //     // set starting position at first byte and read in the raw data
        //     ms.Position = 0;
        //     ms.Read(fileBytes, 0, (int) ms.Length);
        //     // create a file and save the raw zip file to it
        //     FileStream fs = new FileStream(theFile, FileMode.Create);
        //     fs.Write(fileBytes, 0, (int) fileBytes.Length);
        //
        //     fs.Close(); // close the file
        //
        //     FileInfo tempFile = new FileInfo(theFile);
        //
        //     // always good to make sure we actually created the file
        //     if (tempFile.Exists == true)
        //     {
        //         // for now, just delete what we created
        //         tempFile.Delete();
        //     }
        //     else
        //     {
        //         Trace.WriteLine("File was not created!");
        //     }
        // }
    }

    private void SelectStreamFolderClick(object sender, RoutedEventArgs e)
    {
        SelectResourceFolder();
    }

    private void ViewLogClick(object sender, RoutedEventArgs e)
    {
        var logDialog = new LogListDialog(store.LogStore);
        logDialog.Finish();
        logDialog.ShowDialog();
    }

    private void SaveAndGenerateClick(object sender, RoutedEventArgs e)
    {
        store.Options.SaveOptions();
    }

    private void NumberInputText(object sender, TextCompositionEventArgs e)
    {
        Util.NumberInputText(sender, e);
    }

    private void RemoveTextureClick(object sender, RoutedEventArgs e)
    {
        if (TextureList.SelectedItem == null) return;
        var item = (TextureData) TextureList.SelectedItem;
        store.selectedCloth.RemoveTexture(item);
    }

    private void SelectResourceFolder()
    {
        using var folderDialog = new FolderBrowserDialog()
        {
            Description = "Select the dg-clothes folder",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true
        };

        if (folderDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
        {
            MessageBox.Show("Je moet de dg-clothes resource folder selecteren om verder te gaan");
            return;
        }

        Properties.Settings.Default.ClothingResourceFolder = folderDialog.SelectedPath;
        store.Options.data.ResourceFolder = folderDialog.SelectedPath;
        Properties.Settings.Default.Save();
        foreach (var storeClothe in store.Clothes)
        {
           storeClothe.ValidateFileExisting();
        }
    }
}