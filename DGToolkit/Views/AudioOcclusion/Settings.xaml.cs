using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using DGToolkit.Models.AudioOcclusion;
using MessageBox = System.Windows.Forms.MessageBox;
using TextBox = System.Windows.Controls.TextBox;

namespace DGToolkit.Views.AudioOcclusion;

public partial class Settings : Page
{
    public OcclusionModel Model;

    public Settings()
    {
        Model = OcclusionModel.instance;
        InitializeComponent();
        ResourcePath.DataContext = Model.data;
        UpdateSettingFields();
        Model.selected.PropertyChanged += (sender, args) => UpdateSettingFields();
    }

    private void UpdateSettingFields()
    {
        var dctx = Model.selected.Value != null ? Model.data.interiors[Model.selected.Value.Value] : null;
        InteriorName.DataContext = dctx;
        InteriorYmap.DataContext = dctx;
        InteriorYtyp.DataContext = dctx;
    }

    private void SelectResourcePathClick(object sender, RoutedEventArgs e)
    {
        using var folderDialog = new FolderBrowserDialog
        {
            Description = "Select the [assets] folder from the degrens 2.0 repo",
            UseDescriptionForTitle = true,
            SelectedPath = Model.data.assetsPath != ""
                ? Model.data.assetsPath
                : Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
                  + Path.DirectorySeparatorChar,
            ShowNewFolderButton = false
        };

        if (folderDialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        Model.data.assetsPath = folderDialog.SelectedPath;
        ResourcePath.Text = folderDialog.SelectedPath;
    }

    private void SelectYmapPathClick(object sender, RoutedEventArgs e)
    {
        if (Model.selected.Value == null) return;
        if (Model.data.assetsPath == "")
        {
            MessageBox.Show("Select the assets path first!", "Some title",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        using var fileDialog = new OpenFileDialog()
        {
            InitialDirectory = Path.GetDirectoryName(Model.data.interiors[Model.selected.Value.Value].ytypPath),
            Title = "Select the YMAP with the MLO",
            Multiselect = false,
            Filter = "Ymap files (*.ymap)|*.ymap"
        };

        if (fileDialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        Model.data.interiors[Model.selected.Value.Value].ymapPath =
            Path.GetRelativePath(Model.data.assetsPath, fileDialog.FileName);
        Model.entryUpdate();
        InteriorYmap.Text = Model.data.interiors[Model.selected.Value.Value].ymapPath;
    }

    private void SelectYtypPathClick(object sender, RoutedEventArgs e)
    {
        if (Model.selected.Value == null) return;
        if (Model.data.assetsPath == "")
        {
            MessageBox.Show("Select the assets path first!", "Some title",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        using var fileDialog = new OpenFileDialog()
        {
            InitialDirectory = Path.GetDirectoryName(Model.data.interiors[Model.selected.Value.Value].ytypPath),
            Title = "Select the YTYP with the MLO",
            Multiselect = false,
            Filter = "Ytyp files (*.ytyp)|*.ytyp"
        };

        if (fileDialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        Model.data.interiors[Model.selected.Value.Value].ytypPath =
            Path.GetRelativePath(Model.data.assetsPath, fileDialog.FileName);
        InteriorYtyp.Text = Model.data.interiors[Model.selected.Value.Value].ytypPath;
    }
}