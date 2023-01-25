using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using DGToolkit.Models.AudioOcclusion;

namespace DGToolkit.Views.AudioOcclusion;

public partial class Portals : Page
{
    private OcclusionModel _model;

    public Portals()
    {
        _model = OcclusionModel.instance;
        InitializeComponent();
        _model.selected.PropertyChanged += SelectedOnPropertyChanged;
    }

    private void SelectedOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_model.selected.Value == null) return;
        var entry = _model.data.interiors[_model.selected.Value.Value];
        AudioPortals.DataContext = entry.portals;
    }

    private void AudioPortals_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_model.selected.Value == null) return;
        var entry = _model.data.interiors[_model.selected.Value.Value];
        AudioPortals.DataContext = entry.portals;
    }
}