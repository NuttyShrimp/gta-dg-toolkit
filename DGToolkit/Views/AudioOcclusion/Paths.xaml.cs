using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DGToolkit.Models.AudioOcclusion;
using Binding = System.Windows.Data.Binding;

namespace DGToolkit.Views.AudioOcclusion;

public partial class Paths : Page
{
    public OcclusionModel Model;
    public List<ExpandoObject> rows;

    public Paths()
    {
        Model = OcclusionModel.instance;
        rows = new List<ExpandoObject>();
        Model.selected.PropertyChanged += SelectedOnPropertyChanged;
        InitializeComponent();
    }

    private void SelectedOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (Model.selected.Value == null) return;
        rows = new List<ExpandoObject>();
        // First col is text
        var paths = Model.data.interiors[Model.selected.Value.Value].Paths;
        
        //clear rows & columns
        AudioPaths.Columns.Clear();

        //add type Column
        DataGridTextColumn toColumn = new DataGridTextColumn();
        toColumn.Header = "RoomIndex";
        toColumn.Binding = new Binding("RoomIndex");
        AudioPaths.Columns.Add(toColumn);

        //Define rows
        foreach (var (roomTo, routes) in paths)
        {
            var column = new DataGridCheckBoxColumn();
            column.Header = roomTo.ToString();
            column.Binding = new Binding(roomTo.ToString());

            var st = new Style();
            st.TargetType = typeof(DataGridCell);
            // Disable checkbox and set background to gray
            st.Triggers.Add(new DataTrigger
            {
                Binding = toColumn.Binding,
                Value = roomTo.ToString(),
                Setters =
                {
                    new Setter(IsEnabledProperty, false),
                    new Setter(BackgroundProperty, System.Windows.Media.Brushes.DimGray)
                }
            });

            column.CellStyle = st;

            AudioPaths.Columns.Add(column);

            var row = new ExpandoObject() as IDictionary<string, object>;
            row.Add("RoomIndex", roomTo.ToString());
            for (var i = 0; i < paths.Count; i++)
            {
                row.Add(i.ToString(), (object) routes.Contains(i));
            }

            rows.Add(row as ExpandoObject);
        }

        // set rows as items from datagrid
        AudioPaths.ItemsSource = rows;
    }

    private void AudioPaths_OnCellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
    {
        if (e.EditAction == DataGridEditAction.Cancel || Model.selected.Value == null) return;
        var paths = Model.data.interiors[Model.selected.Value.Value].Paths;
        // fromRoom is header of column to int
        var fromRoom = int.Parse(e.Column.Header.ToString());
        if (!paths.ContainsKey(fromRoom))
        {
            paths.Add(fromRoom, new HashSet<int>());
        }

        var toRoomStr = ((IDictionary<string, object>) e.Row.Item)["RoomIndex"] as string;
        if (toRoomStr == null)
        {
            throw new DataException("toRoomStr is null");
        }

        var toRoom = int.Parse(toRoomStr);
        var element = (CheckBox) e.EditingElement;
        if (element.IsChecked == true)
        {
            Debug.WriteLine($"Adding path {toRoom} to {fromRoom}");
            paths[fromRoom].Add(toRoom);
        }
        else
        {
            Debug.WriteLine($"Removing path {toRoom} to {fromRoom}");
            paths[fromRoom].Remove(toRoom);
        }
    }
}