using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using DGToolkit.Models.Clothing;
using DGToolkit.Models.Clothing.Options;
using DGToolkit.Views.AudioPack;
using DataFormats = System.Windows.Forms.DataFormats;

namespace DGToolkit.Views.Clothing;

public partial class ClothingView : Page
{
    public ClothingStore store { get; set; }
    public ContextMenu listMenu { get; set; }

    public ClothingView()
    {
        store = new ClothingStore();

        listMenu = new ContextMenu();
        listMenu.Items.Add("Remove");

        InitializeComponent();
        DrawableList.Visibility = Visibility.Hidden;
        
        AllowDrop = true;
        Drop += HandleFileDrop;
        DLCPackName.DataContext = store.availableDLCS;
        DrawableList.ItemsSource = store.Clothes;
        DrawableList.SelectionChanged += (sender, args) =>
        {
            if (DrawableList.SelectedItem == null)
            {
                DrawableInfoPanel.Visibility = Visibility.Hidden;
                return;
            }

            if (DrawableList.SelectedItem == null) return;
            var item = (ClothData) DrawableList.SelectedItem;
            store.selectedCloth = item;
            DrawableInfoPanel.DataContext = item;
            ClothTypeBox.SelectedValue = item.ClothType;
            DrawableInfoPanel.Visibility = Visibility.Visible;
        };
        DrawableInfoPanel.IsVisibleChanged += (sender, args) =>
        {
            if (DrawableInfoPanel.Visibility != Visibility.Visible) return;
            var clothTypeOptions = Types.ClothTypeDescriptions
                .Select(p => new ComboBoxItem() {Content = p.Value, Tag = p.Key}).ToList();
            ClothTypeBox.ItemsSource = clothTypeOptions;
            var selectedItem =
                clothTypeOptions.FindIndex(p => store.selectedCloth.ClothType == (Types.ClothTypes) p.Tag);
            ClothTypeBox.SelectionChanged += (sender, args) =>
            {
                if (store.selectedCloth == null) return;
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
            ClothTypeBox.SelectedIndex = selectedItem;
        };
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

    private void SelectStreamFolder(object sender, RoutedEventArgs e)
    {
        FolderBrowserDialog dialog = new();
        var dialogResult = dialog.ShowDialog();
        if (dialogResult == System.Windows.Forms.DialogResult.OK)
        {
            store.Options.data.ResourceFolder = dialog.SelectedPath;
            store.Options.SaveOptions();
        }
    }

    private void LoadDLC(object sender, SelectionChangedEventArgs e)
    {
        store.SelectDLC((ShopPedApparel) DLCPackName.SelectedItem);
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
}