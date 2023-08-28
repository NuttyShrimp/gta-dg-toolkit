using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Controls;
using DGToolkit.Models.AudioOcclusion;

namespace DGToolkit.Views.AudioOcclusion;

public partial class Rooms : Page
{
    private OcclusionModel _model;

    public Rooms()
    {
        _model = OcclusionModel.instance;
        InitializeComponent();
        _model.selected.PropertyChanged += SelectedOnPropertyChanged;
    }

    private void SelectedOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_model.selected.Value == null) return;
        var entry = _model.data.interiors[_model.selected.Value.Value];
        AudioRooms.DataContext = entry.Rooms;
    }
}