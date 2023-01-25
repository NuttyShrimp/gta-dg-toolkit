using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using DGToolkit.Models.AudioOcclusion;

namespace DGToolkit.Views.AudioOcclusion;

public partial class AudioOcclusion : Page
{
    private OcclusionModel _model;
    public AudioOcclusion()
    {
        InitializeComponent();
        _model = OcclusionModel.instance;
        InteriorList.DataContext = _model.data.interiors;
        if (_model.selected.Value != null)
        {
            InteriorList.SelectedItem = _model.data.interiors[_model.selected.Value.Value];
        }
    }

    private void CreateInteriorEntryClick(object sender, RoutedEventArgs e)
    {
        _model.createEntry();
    }

    private void ResetButtonClick(object sender, RoutedEventArgs e)
    {
        _model.reset();
    }

    private void SaveButtonClick(object sender, RoutedEventArgs e)
    {
        _model.save();
    }

    private void InteriorList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var entry = InteriorList.SelectedItem as InteriorEntry;
        if (entry == null)
        {
            Debug.WriteLine("Failed to cast sender to InteriorEntry");
            return;
        }
        _model.selectEntry(entry);
    }
}