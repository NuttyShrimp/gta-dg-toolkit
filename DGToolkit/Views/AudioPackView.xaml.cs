﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DGToolkit.Models.AudioPack;
using DGToolkit.Models.AudioPack.Dat54;
using DGToolkit.Views.AudioPack;

namespace DGToolkit.Views
{
    internal class AudioPackName
    {
        public string Name { get; set; }
        public bool Oversized { get; set; }
    }

    /// <summary>
    /// Interaction logic for AudioPack.xaml
    /// </summary>
    public partial class AudioPackView : Page
    {
        private AudioPacks _packs;
        private ObservableCollection<AudioPackName> packNames;

        public AudioPackView()
        {
            InitializeComponent();
            _packs = new AudioPacks();
            _packs.Load();
            packNames = new ObservableCollection<AudioPackName>();
            LoadPackNames();
            DLCPackName.DataContext = packNames;
            if (packNames.Count() != 0)
            {
                DLCPackName.SelectedValue = _packs.state.manifest.packNames[0];
            }
        }

        private void LoadPackNames()
        {
            var names = _packs.state.manifest.packNames;
            foreach (var name in names)
            {
                packNames.Add(new AudioPackName()
                {
                    Name = name,
                    Oversized = _packs.state.oversized.Contains(name),
                });
            }
        }

        private void LoadPack(object sender, SelectionChangedEventArgs e)
        {
            if (DLCPackName.SelectedValue == null)
            {
                AudioEntries.DataContext = new List();
                return;
            }

            AudioEntries.DataContext = _packs.state.manifest.GetPackInfo((string) DLCPackName.SelectedValue).files;
        }

        private void ResetPacks(object sender, RoutedEventArgs e)
        {
            _packs.Load();
            DLCPackName.SelectedValue = null;
            AudioEntries.DataContext = null;
            LoadPackNames();
            if (_packs.state.manifest.packNames.Count != 0)
            {
                DLCPackName.SelectedValue = _packs.state.manifest.packNames[0];
                AudioEntries.DataContext = _packs.state.manifest.GetPackInfo((string) DLCPackName.SelectedValue).files;
            }
        }

        private void SavePack(object sender, RoutedEventArgs e)
        {
            if (DLCPackName.SelectedValue == null) return;
            _packs.Save();
        }

        private void GeneratePacks(object sender, RoutedEventArgs e)
        {
            (sender as Button).IsEnabled = false;
            _packs.GeneratePacks();
            (sender as Button).IsEnabled = true;
        }

        private void OpenHeaderDialog(object sender, RoutedEventArgs e)
        {
            var entry = (FileEntry) (sender as Button).DataContext;
            var dialog = new HeaderDialog(entry.headers.Copy());
            dialog.ShowDialog();
            if (dialog.DialogResult.HasValue && dialog.DialogResult.Value)
            {
                entry.headers = dialog.returnHeaders!;
            }
        }

        private void AudioEntries_OnAddingNewItem(object? sender, AddingNewItemEventArgs e)
        {
            e.NewItem = new FileEntry()
            {
                looped = false,
                name = "",
                headers = new BaseSoundHeader()
                {
                    volume = 100,
                    distance = 5,
                    dopplerFactor = 0,
                    categoryHash = "02C7B342",
                    rolloffHash = "C2770146",
                    echox = 0,
                    echoy = 0,
                    echoz = 0
                }
            };
        }

        private void CreatNewPackClick(object sender, RoutedEventArgs e)
        {
            var dialog = new NewAudioPackDialog();
            dialog.ShowDialog();
            if (dialog.DialogResult.HasValue && dialog.DialogResult.Value)
            {
                _packs.AddPack(dialog.packName);
                DLCPackName.SelectedValue = dialog.packName;
            }
        }
    }
}