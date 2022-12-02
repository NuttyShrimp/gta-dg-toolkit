using System;
using System.Collections.Generic;
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
    /// <summary>
    /// Interaction logic for AudioPack.xaml
    /// </summary>
    public partial class AudioPackView : Page
    {
        private AudioPacks _packs;

        public AudioPackView()
        {
            InitializeComponent();
            _packs = new AudioPacks();
            _packs.Load();
            DLCPackName.DataContext = _packs.state.manifest.packNames;
            DLCPackName.SelectedValue = _packs.state.manifest.packNames[0];
        }

        private void LoadPack(object sender, SelectionChangedEventArgs e)
        {
            AudioEntries.DataContext = _packs.state.manifest.GetPackInfo((string) DLCPackName.SelectedValue).files;
        }

        private void ResetPacks(object sender, RoutedEventArgs e)
        {
            _packs.Load();
            AudioEntries.DataContext = _packs.state.manifest.GetPackInfo((string) DLCPackName.SelectedValue).files;
        }

        private void SavePack(object sender, RoutedEventArgs e)
        {
            if (DLCPackName.SelectedValue == null) return;
            _packs.Save();
        }

        private void OpenHeaderDialog(object sender, RoutedEventArgs e)
        {
            FileEntry entry = (FileEntry) (sender as Button).DataContext;
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
                headers = new baseSoundHeader()
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
    }
}