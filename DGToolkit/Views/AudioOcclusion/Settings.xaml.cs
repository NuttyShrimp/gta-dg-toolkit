using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using DGToolkit.Models.AudioOcclusion;

namespace DGToolkit.Views.AudioOcclusion;

public partial class Settings : Page
{
    private OcclusionModel _model;

    public Settings()
    {
        _model = OcclusionModel.instance;
        InitializeComponent();
        ResourcePath.DataContext = _model.data.assetsPath;
        InteriorName.DataContext = _model.selected != null ? _model.data.interiors[_model.selected.Value].name : "";
        InteriorYmap.DataContext = _model.selected != null ? _model.data.interiors[_model.selected.Value].ymapPath : "";
        InteriorYtyp.DataContext = _model.selected != null ? _model.data.interiors[_model.selected.Value].ytypPath : "";
    }

    private void SelectResourcePathClick(object sender, RoutedEventArgs e)
    {
        using var folderDialog = new FolderBrowserDialog
        {
            Description = "Select the [assets] folder from the degrens 2.0 repo",
            UseDescriptionForTitle = true,
            SelectedPath = _model.data.assetsPath != ""
                ? _model.data.assetsPath
                : Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
                  + Path.DirectorySeparatorChar,
            ShowNewFolderButton = false
        };

        if (folderDialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        _model.data.assetsPath = folderDialog.SelectedPath;
    }
}